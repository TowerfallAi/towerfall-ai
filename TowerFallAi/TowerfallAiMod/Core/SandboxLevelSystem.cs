using System.Text;
using System.Xml;
using Monocle;
using TowerFall;

namespace TowerfallAi.Core {
  public class SandboxLevelSystem : LevelSystem {
		private int[,] solids;
		public QuestLevelData QuestTowerData {
			get;
			private set;
		}

		public SandboxLevelSystem(QuestLevelData tower, int[,] solids) {
			QuestTowerData = tower;
			Theme = tower.Theme;
			ID = tower.ID;
			this.solids = solids;
		}

		public override XmlElement GetNextRoundLevel(MatchSettings matchSettings, int roundIndex, out int randomSeed) {
			randomSeed = this.QuestTowerData.ID.X;
			var xml = Calc.LoadXML(this.QuestTowerData.Path)["level"];
			xml["Entities"].RemoveAll();

			if (solids != null) {
				xml["BG"].InnerText = "00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000";
				xml["BGTiles"].InnerText = "";
				xml.RemoveChild(xml["SolidTiles"]);

				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < solids.GetLength(0); i++) {
					for (int j = 0; j < solids.GetLength(1); j++) {
						sb.Append(solids[i, j]);
					}
					sb.Append('\n');
				}
				xml["Solids"].InnerText = sb.ToString();
			}

			return xml;
		}
	}
}
