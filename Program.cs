// this uses .NET6.0
using Raylib_cs;
using System.Numerics;

Color bgCol = new Color(18, 18, 18, 255); // background color


int size = 50; // width and height of the looping grid area

// list of all cells, with a vec2 as its position and an int as its state
Dictionary<Vector2, int> cells = new Dictionary<Vector2, int>();

// create all the cells needed in the list
for (int y = 0; y < size; y++)
{
  for (int x = 0; x < size; x++)
  {
    cells.Add(new Vector2(x, y), 0);
  }
}
// change these cells states to form glider
cells[new Vector2(5, 5)] = 1;
cells[new Vector2(5, 6)] = 1;
cells[new Vector2(5, 7)] = 1;
cells[new Vector2(4, 7)] = 1;
cells[new Vector2(3, 6)] = 1;

// Raylib window setup
Raylib.InitWindow(size * 10 - 1, size * 10 - 1, "Conway's Game of Life");
Raylib.SetTargetFPS(60);
Raylib.SetExitKey(0);

Vector2 getV(Vector2 v, Vector2 limit)
{
  Vector2 ret = v;

  if (v.X < 0) ret.X = limit.X - 1;
  else if (v.X > limit.X - 1) ret.X = 0;

  if (v.Y < 0) ret.Y = limit.Y - 1;
  else if (v.Y > limit.Y - 1) ret.Y = 0;

  return ret;
}

int getI(int i, int min, int max)
{
  if (i < min) return min;
  if (i > max) return max;
  else return i;
}
// updates each cell using CGoL's rules
Dictionary<Vector2, int> iterate()
{
  Dictionary<Vector2, int> next = new Dictionary<Vector2, int>();

  foreach (var cell in cells)
  {
    int[] neighbors = {
      cells[getV(cell.Key-new Vector2(-1,-1), Vector2.One*size)],
      cells[getV(cell.Key-new Vector2(-1, 0), Vector2.One*size)],
      cells[getV(cell.Key-new Vector2(-1, 1), Vector2.One*size)],

      cells[getV(cell.Key-new Vector2( 0,-1), Vector2.One*size)],

      cells[getV(cell.Key-new Vector2( 0, 1), Vector2.One*size)],

      cells[getV(cell.Key-new Vector2( 1,-1), Vector2.One*size)],
      cells[getV(cell.Key-new Vector2( 1, 0), Vector2.One*size)],
      cells[getV(cell.Key-new Vector2( 1, 1), Vector2.One*size)],
    };

    int zeroCount = 0; // number of neighbors with a state of 0
    int oneCount = 0; // number of neighbors with a state of 1
    foreach (var neighbor in neighbors)
    {
      if (neighbor == 1) oneCount++;
      else zeroCount++;
    }

    if (oneCount < 2 || oneCount > 3) next[cell.Key] = 0;             // death rule
    if (oneCount == 2 || oneCount == 3) next[cell.Key] = cell.Value; // don't change rule
    if (oneCount == 3) next[cell.Key] = 1;                          // birth rule
  }

  return next;
}
// timer stuff for updating every X number of seconds or so
bool hasPaused = false;
void f(Object source, System.Timers.ElapsedEventArgs e)
{
  if (!hasPaused) cells = iterate(); // update cells
}
var aTimer = new System.Timers.Timer();
aTimer.Interval = 200;
aTimer.AutoReset = true;
aTimer.Enabled = true;
aTimer.Elapsed += f;

int interval = 100; // interval in milliseconds

while (!Raylib.WindowShouldClose())
{
  // Raylib stuff 
  Raylib.BeginDrawing();
    Raylib.ClearBackground(bgCol);

    if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)) hasPaused = !hasPaused; // pause and unpause
    if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP)) // speed up time
    {
      interval -= 25;
      aTimer.Interval = getI(interval, 1, 1000);
    }
    if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN)) // slows down time
    {
      interval += 25;
      aTimer.Interval = getI(interval, 1, 1000);
    }

    foreach (var cell in cells) // draw each cell
    {
      Color col;
      if (cell.Value == 1) col = Color.WHITE;
      else col = bgCol;
      Raylib.DrawRectangleV(cell.Key * 10, new Vector2(9, 9), col);
    }
    if (hasPaused) // draw some text to show the game is paused
    {
      string pauseText = "game is paused";
      Raylib.DrawText(pauseText, 10, 10, 20, Color.WHITE);
      Vector2 pauseRecSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), pauseText, 20, 2);
      Raylib.DrawRectangleLines(5, 5, (int)pauseRecSize.X + 10, (int)pauseRecSize.Y + 10, Color.WHITE);
    }

    #region timer text
      string timerText = (((float)(1000 - interval)) / 100).ToString();
      Raylib.DrawText(timerText, 10, 45, 20, Color.WHITE);
      Vector2 timerRecSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), timerText, 20, 2);
      Raylib.DrawRectangleLines(5, 40, (int)timerRecSize.X + 10, (int)timerRecSize.Y + 10, Color.WHITE);
    #endregion

    // get mouse pos relative to grid
    Vector2 mousePos = new Vector2(
      ((int)Raylib.GetMouseX() / 10),
      ((int)Raylib.GetMouseY() / 10)
    );

    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) // changing a cells state
    {
      if (cells[mousePos] == 0) cells[mousePos] = 1;
      else if (cells[mousePos] == 1) cells[mousePos] = 0;
    }
    Raylib.DrawRectangleV(mousePos * 10, new Vector2(9, 9), new Color(255, 255, 255, 128));

  Raylib.EndDrawing();
}
Raylib.CloseWindow();