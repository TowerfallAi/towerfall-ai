using System;
using TowerfallAi.Common;

namespace TowerfallAi.Api {
  public class Vec2 {
    public float x;
    public float y;

    public float Dot(Vec2 v) {
      return x * v.x + y * v.y;
    }

    public Vec2 Add(Vec2 v2) {
      return new Vec2 {
        x = x + v2.x,
        y = y + v2.y
      };
    }

    public Vec2 Sub(Vec2 v2) {
      return new Vec2 {
        x = x - v2.x,
        y = y - v2.y
      };
    }

    public Vec2 Copy() {
      return new Vec2 {
        x = x,
        y = y
      };
    }

    public Vec2 Mul(float a) {
      return new Vec2 {
        x = x * a,
        y = y * a
      };
    }

    public Vec2 Div(float a) {
      return new Vec2 {
        x = x / a,
        y = y / a
      };
    }

    public void Normalize() {
      float length = Length();
      if (length == 0) return;

      x /= length;
      y /= length;
    }

    public float Length() {
      return (float)Math.Sqrt(x * x + y * y);
    }

    public override string ToString() {
      return "({0}, {1})".Format(x, y);
    }
  }
}
