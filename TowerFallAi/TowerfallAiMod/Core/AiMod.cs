using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TowerFall;
using TowerfallAi.Common;
using TowerfallAi.Data;

namespace TowerfallAi.Core {
  /// <summary>
  /// Entry point for the modifications in the game.
  /// </summary>
  public static class AiMod {
    public const string Version = "0.1.1";
    public const string BaseDirectory = "aimod";
    private const string defaultConfigName = "config.json";
    private const string poolName = "default";

    // If this is set to false, this mod should do no effect.
    public static bool Enabled { get; private set;}

    public static readonly Random Random = new Random((int)DateTime.UtcNow.Ticks);

    public static readonly TimeSpan DefaultAgentTimeout = new TimeSpan(0, 0, 10);

    public static string ConfigPath = Util.PathCombine(BaseDirectory, defaultConfigName);

    public static MatchConfig Config { get; private set; }

    private static readonly TimeSpan ellapsedGameTime = new TimeSpan(10000000 / 60);

    private static MatchSettings matchSettings;

    public static ConnectionDispatcher ConnectionDispatcher;

    public static bool IsNoConfig { get { return true; } }
    public static bool IsFastrun { get { return true; } }
    public static bool NoGraphics { get { return false; } }

    private static object ongoingOperationLock = new object();

    private static ReconfigOperation reconfigOperation;
    private static ResetOperation resetOperation;
    private static CancellationTokenSource ctsSession = new CancellationTokenSource();

    static Stopwatch gameTimeWatch;
    private static TimeSpan totalGameTime = new TimeSpan();
    private static long totalFrame = 0;

    private static readonly Stopwatch fpsWatch = new Stopwatch();

    private static bool loggedScreenSize;

    private static bool sessionEnded;

    static Mutex loadContentMutex = new Mutex(false, "Towerfall_loadContent");

    static bool rematch;
    static TimeSpan logTimeInterval = new TimeSpan(0, 1, 0);
    
    public class ReconfigOperation {
      public MatchConfig Config { get; set; }
      public List<RemoteConnection> Connections { get; set; }
    }

    public class ResetOperation {
      public List<JObject> Entities { get; set; }
    }

    public static void ParseArgs(string[] args) {
      for (int i = 0; i < args.Length; i++) {
        if (args[i] == "--aimod") {
          Enabled = true;
        }
      }
    }

    public static void LoadConfigFromPath() {
      if (!File.Exists(ConfigPath)) {
        Logger.Info("No config in {0}. Starting game in normal mode.".Format(ConfigPath));
        Config = new MatchConfig();
        return;
      };

      Logger.Info("Loading config from {0}".Format(AiMod.ConfigPath));
      Config = JsonConvert.DeserializeObject<MatchConfig>(File.ReadAllText(ConfigPath));
    }

    /// <summary>
    /// Called before MonoGame Initialize.
    /// </summary>
    public static void PreGameInitialize() {
      loadContentMutex.WaitOne();
    }

    /// <summary>
    /// Called after MonoGame Initialize.
    /// </summary>
    public static void PostGameInitialize() {
      gameTimeWatch = Stopwatch.StartNew();

      Util.CreateDirectory(BaseDirectory);
      Logger.Init(BaseDirectory);
      Logger.Info($"Mod version: {Version}. Enabled: {Enabled}");

      Agents.Init();
      ctsSession = new CancellationTokenSource();

      if (!IsNoConfig) {
        LoadConfigFromPath();
      }

      Logger.Info("Waiting for game to load.");
      while (!TFGame.GameLoaded) {
        Thread.Sleep(200);
      }

      loadContentMutex.ReleaseMutex();

      ConnectionDispatcher = new ConnectionDispatcher(poolName);

      Logger.Info("Post Game Initialize.");
    }

    public static void Update(Action<GameTime> originalUpdate) {
      int fps = IsMatchRunning() ? Config.fps : 10;
      if (fps > 0) {
        fpsWatch.Stop();
        long ticks = 10000000L / fps;
        if (fpsWatch.ElapsedTicks < ticks) {
          Thread.Sleep((int)(ticks - fpsWatch.ElapsedTicks) / 10000);
        }
        fpsWatch.Reset();
        fpsWatch.Restart();
      }

      if (!ConnectionDispatcher.IsRunning) {
        throw new Exception("ConnectionDispatcher stopped running");
      }

      if (!loggedScreenSize) {
        Logger.Info("Screen: {0} x {1}, {2}".Format(
          TFGame.Instance.Screen.RenderTarget.Width,
          TFGame.Instance.Screen.RenderTarget.Height,
          TFGame.Instance.Screen.RenderTarget.Format));
        loggedScreenSize = true;
      }

      if (PreUpdate()) {
        try {
          originalUpdate(GetGameTime());
        } catch (AggregateException aggregateException) {
          foreach (var innerException in aggregateException.Flatten().InnerExceptions) {
            HandleFailure(innerException);
          }
        } catch (Exception ex) {
          HandleFailure(ex);
        }
      }

      if (gameTimeWatch.ElapsedMilliseconds > logTimeInterval.TotalMilliseconds) {
        LogGameTime();
        gameTimeWatch.Restart();
      }
    }

    public static void HandleFailure(Exception ex) {
      if (ex is SocketException) {
        Logger.Info($"Connection error. Session will stop and wait for another config. Exception:\n  {ex}");
        EndSession();
      } else if (ex is JsonSerializationException || ex is JsonReaderException) {
        Logger.Info($"Serialization error. Session will stop and wait for another config. Exception:\n  {ex}");
        EndSession();
      } else if (ex is OperationCanceledException) {
        Logger.Info($"Task cancelled.\n  {ex}");
      } else if (ex is TimeoutException) {
        Logger.Info($"Task timed out.\n  {ex}");
      } else {
        Logger.Info($"Unhandled exception.\n  {ex}");
        throw ex;
      }
    }

    /// <summary>
    /// Change the config of the game. This will cause the session to restart.
    /// </summary>
    public static void Reconfig(ReconfigOperation operation) {
      lock (ongoingOperationLock) {
        Logger.Info("Cancel ongoing session.");
        ctsSession.Cancel();
        reconfigOperation = operation;
      }
    }

    /// <summary>
    /// Changes the starting configuration of entities. This is faster than a Reconfig.
    /// </summary>
    public static void Reset(ResetOperation operation) {
      lock (ongoingOperationLock) {
        resetOperation = operation;
      }
    }

    public static void Rematch() {
      rematch = true;
    }

    public static void EndSession() {
      if (sessionEnded) return;
      lock (ongoingOperationLock) {
        sessionEnded = true;
        Logger.Info("End Session");
        ctsSession.Cancel();
        Agents.DisconnectAllAgents();
      }
    }

    public static bool IsMatchRunning() {
      if (IsNoConfig) {
        if (Config == null) return false;
        if (Config.agents == null) return false;
        if (Config.agents.Count == 0) return false;
        if (Config.mode == GameModes.Sandbox && !Agents.IsReset) return false;
      }

      if (Config != null &&
          Config.agents != null &&
          Config.agents.Count > 0 &&
          !Agents.Ready) return false;

      return true;
    }

    private static void StartNewSession() {
      Logger.Info("Starting a new session.");
      CreateMatchSettings();
      Session session = new Session(matchSettings);
      session.QuestTestWave = Config.skipWaves;
      session.StartGame();
      Logger.Info("Session started.");
      sessionEnded = false;
      rematch = false;
      Agents.SessionRestarted();
    }

    public static GameTime GetGameTime() {
      return new GameTime(totalGameTime, ellapsedGameTime);
    }

    /// <summary>
    /// This is called before a MonoGame Update. Returns false if the frame should be skipped. This method should not ever be blocked by IO,
    /// otherwise the game window freezes.
    /// </summary>
    public static bool PreUpdate() {
      lock(ongoingOperationLock) {
        // All changes happen in the main thread to avoid race condition during Updates.
        if (ctsSession.IsCancellationRequested) {
          ctsSession = new CancellationTokenSource();
        }

        if (reconfigOperation != null) {
          // Reconfig without a new config works as a Rematch.
          if (reconfigOperation.Config != null) { 
            Config = reconfigOperation.Config;
            Agents.PrepareAgentConnections(Config.agents);
            Agents.AssignRemoteConnections(reconfigOperation.Connections, ctsSession.Token);
          }
          
          reconfigOperation = null;
          StartNewSession();
        } else if (rematch) {
          StartNewSession();
        }

        if (resetOperation != null) {
          Agents.Reset(resetOperation.Entities, ctsSession.Token);
          resetOperation = null;
        }
      }

      if (!IsMatchRunning()) {
        Sound.StopSound();
        return false;
      } else {
        Sound.ResumeSound();
      }

      totalFrame++;
      totalGameTime += ellapsedGameTime;
      return true;
    }

    public static bool IsHumanPlaying() {
      if (Config.mode == null) return true;
      if (NoGraphics) return false;

      foreach (AgentConfig agent in Config.agents) {
        if (agent.type == AgentConfig.Type.Human) {
          return true;
        }
      }

      return false;
    }

    public static void OnExit() {
      if (ConnectionDispatcher != null) {
        ConnectionDispatcher.UnregisterFromPool();
      }

      Logger.Info($"Game process ended. Pid: {Process.GetCurrentProcess().Id}");
    }

    private static void CreateMatchSettings() {
      if (!IsNoConfig) {
        Config = JsonConvert.DeserializeObject<MatchConfig>(File.ReadAllText(ConfigPath));
      }
      
      if (Config.mode == GameModes.Versus) {
        Logger.Info("Configuring Versus mode.");
        LevelSystem levelSystem = GameData.VersusTowers[Config.level].GetLevelSystem();
        matchSettings = new MatchSettings(levelSystem, Modes.TeamDeathmatch, MatchSettings.MatchLengths.Standard);
        matchSettings.Variants.TournamentRules();
      } else if (Config.mode == GameModes.Quest) {
        Logger.Info("Configuring Quest mode.");
        matchSettings = MatchSettings.GetDefaultQuest();
        matchSettings.LevelSystem = GameData.QuestLevels[Config.level].GetLevelSystem();
      } else if (Config.mode == GameModes.Sandbox) {
        Logger.Info("Configuring Sandbox mode.");
        matchSettings = MatchSettings.GetDefaultTrials();
        matchSettings.Mode = Modes.LevelTest;
        matchSettings.LevelSystem = new SandboxLevelSystem(GameData.QuestLevels[Config.level], Config.solids);
      } else {
        throw new Exception("Game mode not supported: {0}".Format(Config.mode));
      }

      for (int i = 0; i < Config.agents.Count; i++) {
        var agent = Config.agents[i];
        TFGame.Players[i] = true;
        TFGame.Characters[i] = agent.GetArcherIndex();
        TFGame.AltSelect[i] = agent.GetArcherType();
        matchSettings.Teams[i] = agent.GetTeam();
      }
    }

    public static void ValidateConfig(MatchConfig config) {
      if (config.mode == null && IsNoConfig) {
        throw new ConfigException("Game mode need to be specified in config request.");
      }

      if (config.mode == null && IsFastrun) {
        throw new ConfigException("Fastrun can only be enabled when game mode is selected.");
      }

      if (config.agents == null || config.agents.Count <= 0) {
        Logger.Info("No agent in config, starting normal game.");
        return;
      }

      if (config.agents.Count > 4) {
        throw new ConfigException("Too many agents. Only 4 bots are supported.");
      }

      if (config.level <= 0) {
        Logger.Info("Invalid level {0}. Defaulting to level 1.".Format(config.level));
      }

      if (config.agentTimeout == default) {
        Logger.Info($"Agent timeout not specified. Using default {DefaultAgentTimeout}.");
        config.agentTimeout = DefaultAgentTimeout;
      }
    }

    private static void LogGameTime() {
      Logger.Info("{0}s, {1} frames".Format((long)GetGameTime().TotalGameTime.TotalSeconds, totalFrame));
    }
  }
}
