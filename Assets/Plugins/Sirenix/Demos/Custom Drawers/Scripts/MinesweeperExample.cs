#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos
{
    using System;
    using UnityEngine;
    using Sirenix.Utilities;

#if UNITY_EDITOR

    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;

#endif

    public class MinesweeperExample : MonoBehaviour
    {
        [Minesweeper]
        public int NumberOfBombs;
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class MinesweeperAttribute : Attribute
    { }

#if UNITY_EDITOR

    /// <summary>
    /// Minesweeper.
    /// </summary>
    public sealed class MinesweeperAttributeDrawer : OdinAttributeDrawer<MinesweeperAttribute, int>
    {
        private enum Tile
        {
            Empty = 0, // Empty tile.

            // 1-8.

            Open = 9,
            Bomb = 10,
            Flag = 11,
        }

        private readonly Color[] NumberColors = new Color[8]
        {
            new Color32(42, 135, 238, 255),		// 1
			new Color32(57, 233, 48, 255),		// 2
			new Color32(253, 0, 0, 255),		// 3
			new Color32(31, 23, 173, 255),		// 4
			new Color32(36, 30, 155, 255),		// 5
			new Color32(131, 29, 29, 255),		// 6
			new Color32(40, 40, 40, 255),		// 7
			new Color32(132, 132, 132, 255),    // 8
		};

        private const float TileSize = 20;
        private const int BoardSize = 25;

        private readonly object Key = new object();

        private bool isRunning;
        private bool gameOver;
        private int flaggedBombs;
        private int numberOfBombs;
        private Tile[,] visibleTiles;
        private Tile[,] tiles;
        private double time;
        private double prevTime;

        protected override void Initialize()
        {
            this.isRunning = false;
            this.visibleTiles = new Tile[BoardSize, BoardSize];
            this.tiles = new Tile[BoardSize, BoardSize];
        }
        
        private void StartGame(int bombs)
        {
            this.numberOfBombs = bombs;

            //��l�ƴѽL
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    this.visibleTiles[x, y] = Tile.Empty;
                    this.tiles[x, y] = Tile.Empty;
                }
            }

            // Spawn bombs.
            for (int count = 0; count < this.numberOfBombs;)
            {
                int x = UnityEngine.Random.Range(0, BoardSize);
                int y = UnityEngine.Random.Range(0, BoardSize);

                //�H���줣�O���u�~�]�w�����u
                if (this.tiles[x, y] != Tile.Bomb)
                {
                    this.tiles[x, y] = Tile.Bomb;

                    //����K�椣�O���u���ܴN�Ʀr+1
                    if (x + 1 < BoardSize && this.tiles[x + 1, y] != Tile.Bomb)
                    {
                        this.tiles[x + 1, y] = (Tile)((int)this.tiles[x + 1, y] + 1);
                    }
                    if (x + 1 < BoardSize && y + 1 < BoardSize && this.tiles[x + 1, y + 1] != Tile.Bomb)
                    {
                        this.tiles[x + 1, y + 1] = (Tile)((int)this.tiles[x + 1, y + 1] + 1);
                    }
                    if (y + 1 < BoardSize && this.tiles[x, y + 1] != Tile.Bomb)
                    {
                        this.tiles[x, y + 1] = (Tile)((int)this.tiles[x, y + 1] + 1);
                    }
                    if (x - 1 >= 0 && y + 1 < BoardSize && this.tiles[x - 1, y + 1] != Tile.Bomb)
                    {
                        this.tiles[x - 1, y + 1] = (Tile)((int)this.tiles[x - 1, y + 1] + 1);
                    }

                    if (x - 1 >= 0 && this.tiles[x - 1, y] != Tile.Bomb)
                    {
                        this.tiles[x - 1, y] = (Tile)((int)this.tiles[x - 1, y] + 1);
                    }
                    if (x - 1 >= 0 && y - 1 >= 0 && this.tiles[x - 1, y - 1] != Tile.Bomb)
                    {
                        this.tiles[x - 1, y - 1] = (Tile)((int)this.tiles[x - 1, y - 1] + 1);
                    }
                    if (y - 1 >= 0 && this.tiles[x, y - 1] != Tile.Bomb)
                    {
                        this.tiles[x, y - 1] = (Tile)((int)this.tiles[x, y - 1] + 1);
                    }
                    if (x + 1 < BoardSize && y - 1 >= 0 && this.tiles[x + 1, y - 1] != Tile.Bomb)
                    {
                        this.tiles[x + 1, y - 1] = (Tile)((int)this.tiles[x + 1, y - 1] + 1);
                    }

                    count++;
                }
            }

            this.gameOver = false;
            this.isRunning = true;
            this.flaggedBombs = 0;
            this.prevTime = EditorApplication.timeSinceStartup;
            this.time = 0.0;
        }

        /// <summary>
        /// Handles the Minesweeper game.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            //�o��function�|�@���]
            Rect rect = EditorGUILayout.GetControlRect();
            this.ValueEntry.SmartValue = Mathf.Clamp(SirenixEditorFields.IntField(rect.AlignLeft(rect.width - 80 - 4), "Number of Bombs", this.ValueEntry.SmartValue), 1, (BoardSize * BoardSize) / 4);

            // Start game
            if (GUI.Button(rect.AlignRight(80), "Start"))
            {
                this.StartGame(this.ValueEntry.SmartValue);
            }

            // Game
            SirenixEditorGUI.BeginShakeableGroup(this.Key);
            if (this.isRunning)
            {
                //Debug.Log("Game");
                //�o�̷|�@������
                this.Game();
            }
            SirenixEditorGUI.EndShakeableGroup(this.Key);
        }

        private void Game()
        {
            Rect rect = EditorGUILayout.GetControlRect(true, TileSize * BoardSize + 20);
            rect = rect.AlignCenter(TileSize * BoardSize);

            // Toolbar
            {
                SirenixEditorGUI.DrawSolidRect(rect.AlignTop(20), new Color(0.5f, 0.5f, 0.5f, 1f));
                SirenixEditorGUI.DrawBorders(rect.AlignTop(20).SetHeight(21).SetWidth(rect.width + 1), 1);

                if (Event.current.type == EventType.Repaint && !this.gameOver)
                {
                    double t = EditorApplication.timeSinceStartup;
                    this.time += t - this.prevTime;
                    this.prevTime = t;
                }

                var time = GUIHelper.TempContent(((int)this.time).ToString());
                GUIHelper.PushContentColor(Color.black);
                GUI.Label(rect.AlignTop(20).HorizontalPadding(4).AlignMiddle(18).AlignRight(EditorStyles.label.CalcSize(time).x), time);
                GUIHelper.PopContentColor();

                GUIHelper.PushColor(Color.yellow);
                GUI.Label(rect.AlignTop(20).AlignCenter(20), EditorIcons.PacmanGhost.Raw);
                GUIHelper.PopColor();

                if (this.gameOver)
                {
                    GUIHelper.PushContentColor(this.flaggedBombs == this.numberOfBombs ? Color.green : Color.red);
                    GUI.Label(rect.AlignTop(20).HorizontalPadding(4).AlignMiddle(18), this.flaggedBombs == this.numberOfBombs ? "You win!" : "Game over!");
                    GUIHelper.PopContentColor();
                }
            }

            rect = rect.AlignBottom(rect.height - 20);
            SirenixEditorGUI.DrawSolidRect(rect, new Color(0.7f, 0.7f, 0.7f, 1f));

            //�}�l�ˬd�C�@��A�@����s���e�A�o�Oeditor�W�����k
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                Rect tileRect = rect.SplitGrid(TileSize, TileSize, i);
                SirenixEditorGUI.DrawBorders(tileRect.SetWidth(tileRect.width + 1).SetHeight(tileRect.height + 1), 1);

                //�Τ@��for�j����M��ӴѽL�Ax�My���κ⪺�A�ѽL������ܦa�M�u�ꪺ��Ӹ��
                int x = i % BoardSize;
                int y = i / BoardSize;
                var tile = this.tiles[x, y];
                var visible = this.visibleTiles[x, y];

                if (this.gameOver || visible == Tile.Open)
                {
                    SirenixEditorGUI.DrawSolidRect(new Rect(tileRect.x + 1, tileRect.y + 1, tileRect.width - 1, tileRect.height - 1), new Color(0.3f, 0.3f, 0.3f, 1f));
                }

                //��ܬ��u
                if ((this.gameOver || visible == Tile.Open) && tile == Tile.Bomb)
                {
                    GUIHelper.PushColor(visible == Tile.Flag ? Color.black : Color.white);
                    GUI.Label(tileRect.AlignCenter(18).AlignMiddle(18), EditorIcons.SettingsCog.ActiveGUIContent);
                    GUIHelper.PopColor();
                }

                //��ܺX�l
                if (visible == Tile.Flag)
                {
                    GUIHelper.PushColor(Color.red);
                    GUI.Label(tileRect.AlignCenter(18).AlignMiddle(18), EditorIcons.Flag.ActiveGUIContent);
                    GUIHelper.PopColor();
                }

                //��ܼƦr
                if ((this.gameOver || visible == Tile.Open) && (int)tile >= 1 && (int)tile <= 8)
                {
                    GUIHelper.PushColor(this.NumberColors[(int)tile - 1]);
                    GUI.Label(tileRect.AlignCenter(18).AlignCenter(18).AddX(2).AddY(2), ((int)tile).ToString(), EditorStyles.boldLabel);
                    GUIHelper.PopColor();
                }

                //�����I��
                if (!this.gameOver && tileRect.Contains(Event.current.mousePosition))
                {
                    SirenixEditorGUI.DrawSolidRect(new Rect(tileRect.x + 1, tileRect.y + 1, tileRect.width - 1, tileRect.height - 1), new Color(0f, 1f, 0f, 0.3f));

                    // Input
                    // Reveal
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (visible != Tile.Flag)
                        {
                            if (tile == Tile.Bomb)
                            {
                                // LOSE
                                //�I�쬵�u�N��A�i�Jgame over
                                this.gameOver = true;
                                SirenixEditorGUI.StartShakingGroup(this.Key, 3f);
                            }
                            else
                            {
                                //½�}�Ӯ�ѽL
                                this.Reveal(x, y);
                            }
                        }

                        Event.current.Use();
                    }
                    // Place flag
                    else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    {
                        //��ܪ������O�S���}���~��]�m�X�l
                        if (visible == Tile.Empty)
                        {
                            this.visibleTiles[x, y] = Tile.Flag;

                            if (tile == Tile.Bomb)
                            {
                                //�p�G�Ӯ�u���ƬO���u�A�N�n���Ѭ��u���ƶq�W�[
                                this.flaggedBombs++;
                                //�Ѩ쪺���u�ƶq�M�]�w�����u�ƶq�ۦP�A�C������
                                if (this.flaggedBombs == this.numberOfBombs)
                                {
                                    this.gameOver = true;
                                }
                            }
                        }
                        else if (visible == Tile.Flag)
                        {
                            //�I�b�쥻�O�X�l����l�W�A�Ӯ�����ܦ^���}�Ҫ���l
                            this.visibleTiles[x, y] = Tile.Empty;
                            //����q�쬵�u�o�����A�n��Ѭ��u�ƶq���^��
                            if (tile == Tile.Bomb)
                            {
                                this.flaggedBombs--;
                            }
                        }

                        Event.current.Use();
                    }
                }
            }

            GUIHelper.RequestRepaint();
        }

        private void Reveal(int x, int y)
        {
            //½�}(x, y)��m
            //�W�L�ѽL�d��A���B�z
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
            {
                return;
            }

            //�w�g���}���ťծ�A���B�z
            if (this.visibleTiles[x, y] == Tile.Open)
            {
                return;
            }

            //�Ӯ�O���u�A��½�}�A�o��|�O�I����l�X�����ɭԧP�_
            if (this.tiles[x, y] == Tile.Bomb)
            {
                return;
            }

            //�Ӯ�O�Ʀr�ΪťաA���}�����
            if ((int)this.tiles[x, y] <= 8)
            {
                this.visibleTiles[x, y] = Tile.Open;

                //�p�G�O�Ʀr���ܴN���~�򩹤U��
                if (this.tiles[x, y] != Tile.Empty)
                {
                    return;
                }
            }

            //�X��½�}��L��l�A���j�h��
            // Recursive reveal.
            this.Reveal(x + 1, y);
            this.Reveal(x + 1, y + 1);
            this.Reveal(x, y + 1);
            this.Reveal(x - 1, y + 1);

            this.Reveal(x - 1, y);
            this.Reveal(x - 1, y - 1);
            this.Reveal(x, y - 1);
            this.Reveal(x + 1, y - 1);
        }
    }

#endif
}
#endif
