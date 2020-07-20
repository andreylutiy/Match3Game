using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Match3Game
{
    public class GameUiController : MonoBehaviour, Match3Lib.IGameEventsListener
    {
        const string MovingAnimationName = "MovingClip";
        readonly Vector2Int SelectedItemZeroPos = new Vector2Int(-1, -1);

        private int Width
        {
            get
            {
                if (CellItems == null)
                {
                    return 0;
                }

                return CellItems.GetLength(0);
            }
        }

        private int Height
        {
            get
            {
                if (CellItems == null)
                {
                    return 0;
                }

                return CellItems.GetLength(1);
            }
        }

        [SerializeField] private RectTransform GamePanel;
        [SerializeField] private RectTransform BackgroundItemsPanel;
        [SerializeField] private RectTransform GameItemsPanel;
        [SerializeField] private RectTransform BangItemsPanel;
        [SerializeField] private RectTransform selectedItemPointer;
        [SerializeField] private UnityEngine.UI.Text ScoreText;
        [SerializeField] private UnityEngine.UI.Text TimeText;
        [SerializeField] private GameObject[] CellItemPrefabs;
        [SerializeField] private GameObject CellBangItemPrefab;
        [SerializeField] private GameObject CellBackgroundPrefab;
        [SerializeField] private float CreateDelay = 0.15f;
        [SerializeField] private float MoveDelay = 0.5f;
        [SerializeField] private float DestroyDelay = 0.3f;

        private GameObject[] CellBackgrounds = new GameObject[0];
        private GameObject[,] CellItems = new GameObject[0, 0];
        private Vector2Int SelectedItemPos = new Vector2Int(-1, -1);

        // bad name and signature
        // need to change this logic
        private System.Action<int, int, int, int> MakeStepAction =
            new System.Action<int, int, int, int>((x0, y0, x1, y1) => { });

        private int CurrentStep = -1;
        private int CurrentStepValue = 0;
        private int CurrentStepMaxValue = 0;
        private Dictionary<int, List<System.Action>> GameStepAction = new Dictionary<int, List<System.Action>>();

        private void Update()
        {
            var isNextStep = GameStepAction.Keys.Where(item => item > CurrentStep).Count() > 0;

            if (GameStepAction.ContainsKey(CurrentStep))
                isNextStep = isNextStep && CurrentStepValue == CurrentStepMaxValue;

            if (isNextStep)
            {
                GameStepAction.Remove(CurrentStep);
                CurrentStep++;
                CurrentStepValue = 0;

                if (GameStepAction.ContainsKey(CurrentStep))
                    CurrentStepMaxValue = GameStepAction[CurrentStep].Count;
                else
                    CurrentStepMaxValue = 0;
            }

            if (GameStepAction.ContainsKey(CurrentStep))
            {
                var actualActions = GameStepAction[CurrentStep];

                if (actualActions.Count <= 0)
                    return;

                lock (actualActions)
                {
                    for (int i = 0; i < actualActions.Count; i++)
                        actualActions[i]();

                    actualActions.Clear();
                }
            }
        }

        public void StartGame(Vector2Int size, System.Action<int, int, int, int> makeStepAction)
        {
            selectedItemPointer.gameObject.SetActive(false);
            GamePanel.sizeDelta = new Vector2(GamePanel.rect.height * (size.x * 1.0f / size.y), GamePanel.sizeDelta.y);
            CreateDelay = CreateDelay / (0.5f * (size.x + size.y));
            this.MakeStepAction = makeStepAction;

            for (int i = 0; i < CellBackgrounds.Length; i++)
                Destroy(CellBackgrounds[i]);

            for (int i = 0; i < Width; i++)
            for (int j = 0; j < Height; j++)
                Destroy(CellItems[i, j]);

            CellBackgrounds = new GameObject[size.x * size.y];
            CellItems = new GameObject[size.x, size.y];

            for (int i = 0; i < size.y; i++)
            for (int j = 0; j < size.x; j++)
            {
                var currentCell = CopyCell(CellBackgroundPrefab, BackgroundItemsPanel, i, j, size.x, size.y);
                CellBackgrounds[i * size.x + j] = currentCell;
            }

            MoveToCell(0, 0, size.x, size.y, ref selectedItemPointer);
        }

        public void OnCellClick(int x, int y)
        {
            if (SelectedItemPos == SelectedItemZeroPos)
            {
                SelectedItemPos = new Vector2Int(x, y);
                selectedItemPointer.gameObject.SetActive(true);
                MoveToCell(x, y, Width, Height, ref selectedItemPointer);
            }
            else
            {
                selectedItemPointer.gameObject.SetActive(false);
                MakeStepAction(SelectedItemPos.x, SelectedItemPos.y, x, y);
                SelectedItemPos = SelectedItemZeroPos;
            }
        }

        public void OnTimeChange(float time)
        {
            TimeText.text = time.ToString();
        }

        void Match3Lib.IGameEventsListener.OnCellCreate(Match3Lib.Point point, int cellType, int gameStep)
        {
            AddGameStepAction(gameStep, new System.Action(() =>
            {
                var movingDelay = CreateDelay * (Height + point.y);
                try
                {
                    CellItems[point.x, point.y] = CopyCell(CellItemPrefabs[cellType], GameItemsPanel, point.x, point.y,
                        Width, Height);
                    SetCellCallback(point.x, point.y, CellItems[point.x, point.y]);
                    MoveCellByAnim(point.x, Height + point.y, point.x, point.y, Width, Height, movingDelay,
                        CellItems[point.x, point.y]);
                }
                finally
                {
                    Invoke("GameStepActionDone", movingDelay);
                }
            }));
        }

        void Match3Lib.IGameEventsListener.OnCellDestroy(Match3Lib.Point point, int gameStep)
        {
            AddGameStepAction(gameStep, new System.Action(() =>
            {
                try
                {
                    var bangItem = Instantiate(CellBangItemPrefab);
                    var bangItemTransform = bangItem.GetComponent<RectTransform>();
                    bangItemTransform.parent = BangItemsPanel;
                    bangItemTransform.offsetMin = CellItems[point.x, point.y].GetComponent<RectTransform>().offsetMin;
                    bangItemTransform.offsetMax = CellItems[point.x, point.y].GetComponent<RectTransform>().offsetMax;
                    MoveToCell(point.x, point.y, Width, Height, ref bangItemTransform);
                    Destroy(CellItems[point.x, point.y]);
                }
                finally
                {
                    Invoke("GameStepActionDone", DestroyDelay);
                }
            }));
        }

        void Match3Lib.IGameEventsListener.OnCellMove(Match3Lib.Point oldPos, Match3Lib.Point newPos, int gameStep)
        {
            AddGameStepAction(gameStep, new System.Action(() =>
            {
                try
                {
                    var tempCell = CellItems[oldPos.x, oldPos.y];
                    CellItems[oldPos.x, oldPos.y] = CellItems[newPos.x, newPos.y];
                    CellItems[newPos.x, newPos.y] = tempCell;
                    SetCellCallback(oldPos.x, oldPos.y, CellItems[oldPos.x, oldPos.y]);
                    SetCellCallback(newPos.x, newPos.y, CellItems[newPos.x, newPos.y]);
                    if (CellItems[oldPos.x, oldPos.y] != null)
                    {
                        var cellItemsAmin = CellItems[oldPos.x, oldPos.y].GetComponent<Animation>();
                        if (cellItemsAmin != null)
                        {
                            if (cellItemsAmin.GetClip(MovingAnimationName) != null)
                                cellItemsAmin.RemoveClip(MovingAnimationName);

                            var movingClip = GetMovingClip(newPos.x, newPos.y, oldPos.x, oldPos.y, Width, Height,
                                MoveDelay);
                            cellItemsAmin.AddClip(movingClip, MovingAnimationName);
                            cellItemsAmin.Play(MovingAnimationName);
                        }
                    }

                    if (CellItems[newPos.x, newPos.y] != null)
                    {
                        var cellItemsAmin = CellItems[newPos.x, newPos.y].GetComponent<Animation>();
                        if (cellItemsAmin != null)
                        {
                            if (cellItemsAmin.GetClip(MovingAnimationName) != null)
                                cellItemsAmin.RemoveClip(MovingAnimationName);

                            var movingClip = GetMovingClip(oldPos.x, oldPos.y, newPos.x, newPos.y, Width, Height,
                                MoveDelay);
                            cellItemsAmin.AddClip(movingClip, MovingAnimationName);
                            cellItemsAmin.Play(MovingAnimationName);
                        }
                    }
                }
                finally
                {
                    Invoke("GameStepActionDone", MoveDelay);
                }
            }));
        }

        void Match3Lib.IGameEventsListener.OnScore(int newScore, int gameStep)
        {
            AddGameStepAction(gameStep, new System.Action(() =>
            {
                try
                {
                    ScoreText.text = newScore.ToString();
                }
                finally
                {
                    Invoke("GameStepActionDone", 0);
                }
            }));
        }

        private void AddGameStepAction(int gameStep, System.Action targetAction)
        {
            if (!GameStepAction.ContainsKey(gameStep))
                GameStepAction.Add(gameStep, new List<System.Action>());

            GameStepAction[gameStep].Add(targetAction);

            if (gameStep == CurrentStep)
                CurrentStepMaxValue++;
        }

        private void GameStepActionDone()
        {
            this.CurrentStepValue++;
        }

        private void SetCellCallback(int x, int y, GameObject targetCellObj)
        {
            if (targetCellObj == null)
                return;

            var cellButton = targetCellObj.GetComponent<UnityEngine.UI.Button>();

            if (cellButton != null)
            {
                var cellPos = new Vector2Int(x, y);
                cellButton.onClick.RemoveAllListeners();
                cellButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                {
                    OnCellClick(cellPos.x, cellPos.y);
                }));
            }
        }

        private static GameObject CopyCell(GameObject prefab, Transform parent, int x, int y, int width, int height)
        {
            var result = Instantiate(prefab);
            var prefabRect = prefab.GetComponent<RectTransform>();
            var resultRect = result.GetComponent<RectTransform>();

            if (resultRect != null)
            {
                resultRect.parent = parent;
                resultRect.offsetMin = prefabRect.offsetMin;
                resultRect.offsetMax = prefabRect.offsetMax;
                MoveToCell(x, y, width, height, ref resultRect);
            }

            return result;
        }

        private static void MoveToCell(int x, int y, int width, int height, ref RectTransform item)
        {
            var offsetMin = new Vector2(item.offsetMin.x, item.offsetMin.y);
            var offsetMax = new Vector2(item.offsetMax.x, item.offsetMax.y);

            item.anchorMin = new Vector2(x * 1.0f / width, y * 1.0f / height);
            item.anchorMax = new Vector2((x + 1) * 1.0f / width, (y + 1) * 1.0f / height);
            item.offsetMin = offsetMin;
            item.offsetMax = offsetMax;
        }

        private static void MoveCellByAnim(int oldX, int oldY, int newX, int newY, int width, int height, float time,
            GameObject cellItem)
        {
            if (cellItem != null)
            {
                var cellItemAmin = cellItem.GetComponent<Animation>();
                if (cellItemAmin != null)
                {
                    if (cellItemAmin.GetClip(MovingAnimationName) != null)
                        cellItemAmin.RemoveClip(MovingAnimationName);

                    cellItemAmin.AddClip(GetMovingClip(oldX, oldY, newX, newY, width, height, time),
                        MovingAnimationName);
                    cellItemAmin.Play(MovingAnimationName);
                }
            }
        }

        private static AnimationClip GetMovingClip(float startX, float startY, float endX, float endY, int width,
            int height,
            float time)
        {
            var result = new AnimationClip();
            result.legacy = true;
            result.SetCurve("", typeof(RectTransform), "m_AnchorMin.x",
                GetAnimationCurve(startX / width, endX / width, time));
            result.SetCurve("", typeof(RectTransform), "m_AnchorMin.y",
                GetAnimationCurve(startY / width, endY / width, time));
            result.SetCurve("", typeof(RectTransform), "m_AnchorMax.x",
                GetAnimationCurve((startX + 1) / width, (endX + 1) / width, time));
            result.SetCurve("", typeof(RectTransform), "m_AnchorMax.y",
                GetAnimationCurve((startY + 1) / width, (endY + 1) / width, time));
            result.name = MovingAnimationName;

            return result;
        }

        private static AnimationCurve GetAnimationCurve(float startValue, float endValue, float time)
        {
            var result = new AnimationCurve();
            result.AddKey(0, startValue);
            result.AddKey(time, endValue);

            return result;
        }
    }
}