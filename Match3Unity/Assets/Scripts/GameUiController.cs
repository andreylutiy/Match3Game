using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameUiController : MonoBehaviour, Match3Lib.IGameEventsListener
{
    const string movingAnimationName = "MovingClip";
    readonly Vector2Int selectedItemZeroPos = new Vector2Int(-1, -1);

    int Width { get { return cellItems == null ? 0 : cellItems.GetLength(0); } }
    int Height { get { return cellItems == null ? 0 : cellItems.GetLength(1); } }

    [SerializeField]
    RectTransform gamePanel;
    [SerializeField]
    RectTransform backgroundItemsPanel;
    [SerializeField]
    RectTransform gameItemsPanel;
    [SerializeField]
    RectTransform bangItemsPanel;
    [SerializeField]
    RectTransform selectedItemPointer;
    [SerializeField]
    UnityEngine.UI.Text scoreText;
    [SerializeField]
    UnityEngine.UI.Text timeText;
    [SerializeField]
    GameObject[] cellItemPrefabs;
    [SerializeField]
    GameObject cellBangItemPrefab;
    [SerializeField]
    GameObject cellBackgroundPrefab;
    [SerializeField]
    float createDelay = 0.15f;
    [SerializeField]
    float moveDelay = 0.5f;
    [SerializeField]
    float destroyDelay = 0.3f;

    GameObject[] cellBackgrounds = new GameObject[0];
    GameObject[,] cellItems = new GameObject[0, 0];
    Vector2Int selectedItemPos = new Vector2Int(-1, -1);
    System.Action<int, int, int, int> makeStepAction = new System.Action<int, int, int, int>((x0, y0, x1, y1) => { });
    int currentStep = -1;
    int currentStepValue = 0;
    int currentStepMaxValue = 0;
    Dictionary<int, List<System.Action>> gameStepAction = new Dictionary<int, List<System.Action>>();

    private void Update()
    {
        var isNextStep = gameStepAction.Keys.Where(item => item > currentStep).Count() > 0;

        if (gameStepAction.ContainsKey(currentStep))
            isNextStep = isNextStep && currentStepValue == currentStepMaxValue;

        if (isNextStep)
        {
            gameStepAction.Remove(currentStep);
            currentStep++;
            currentStepValue = 0;

            if (gameStepAction.ContainsKey(currentStep))
                currentStepMaxValue = gameStepAction[currentStep].Count;
            else
                currentStepMaxValue = 0;
        }

        if (gameStepAction.ContainsKey(currentStep))
        {
            var actualActions = gameStepAction[currentStep];

            if (actualActions.Count > 0)
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
        gamePanel.sizeDelta = new Vector2(gamePanel.rect.height * (size.x * 1.0f / size.y), gamePanel.sizeDelta.y);
        createDelay = createDelay / (0.5f * (size.x + size.y));
        this.makeStepAction = makeStepAction;

        for (int i = 0; i < cellBackgrounds.Length; i++)
            Destroy(cellBackgrounds[i]);

        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Height; j++)
                Destroy(cellItems[i, j]);

        cellBackgrounds = new GameObject[size.x * size.y];
        cellItems = new GameObject[size.x, size.y];

        var prefabTransform = cellBackgroundPrefab.GetComponent<RectTransform>();

        for (int i = 0; i < size.y; i++)
            for (int j = 0; j < size.x; j++)
            {
                var currentCell = CopyCell(cellBackgroundPrefab, backgroundItemsPanel, i, j, size.x, size.y);
                cellBackgrounds[i * size.x + j] = currentCell;
            }

        MoveToCell(0, 0, size.x, size.y, ref selectedItemPointer);
    }

    public void OnCellClick(int x, int y)
    {
        if (selectedItemPos == selectedItemZeroPos)
        {
            selectedItemPos = new Vector2Int(x, y);
            selectedItemPointer.gameObject.SetActive(true);
            MoveToCell(x, y, Width, Height, ref selectedItemPointer);
        }
        else
        {
            selectedItemPointer.gameObject.SetActive(false);
            makeStepAction(selectedItemPos.x, selectedItemPos.y, x, y);
            selectedItemPos = selectedItemZeroPos;
        }
    }

    public void OnTimeChange(float time)
    {
        timeText.text = time.ToString();
    }

    void Match3Lib.IGameEventsListener.OnCellCreate(Match3Lib.Point point, int cellType, int gameStep)
    {
        AddGameStepAction(gameStep, new System.Action(() =>
        {
            var movingDelay = createDelay * (Height + point.y);
            try
            {
                cellItems[point.x, point.y] = CopyCell(cellItemPrefabs[cellType], gameItemsPanel, point.x, point.y, Width, Height);
                SetCellCallback(point.x, point.y, cellItems[point.x, point.y]);
                MoveCellByAnim(point.x, Height + point.y, point.x, point.y, Width, Height, movingDelay, cellItems[point.x, point.y]);
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
                var bangItem = Instantiate(cellBangItemPrefab);
                var bangItemTransform = bangItem.GetComponent<RectTransform>();
                bangItemTransform.parent = bangItemsPanel;
                bangItemTransform.offsetMin = cellItems[point.x, point.y].GetComponent<RectTransform>().offsetMin;
                bangItemTransform.offsetMax = cellItems[point.x, point.y].GetComponent<RectTransform>().offsetMax;
                MoveToCell(point.x, point.y, Width, Height, ref bangItemTransform);
                Destroy(cellItems[point.x, point.y]);
            }
            finally
            {
                Invoke("GameStepActionDone", destroyDelay);
            }
        }));
    }

    void Match3Lib.IGameEventsListener.OnCellMove(Match3Lib.Point oldPos, Match3Lib.Point newPos, int gameStep)
    {
        AddGameStepAction(gameStep, new System.Action(() =>
        {
            try
            {
                var tempCell = cellItems[oldPos.x, oldPos.y];
                cellItems[oldPos.x, oldPos.y] = cellItems[newPos.x, newPos.y];
                cellItems[newPos.x, newPos.y] = tempCell;
                SetCellCallback(oldPos.x, oldPos.y, cellItems[oldPos.x, oldPos.y]);
                SetCellCallback(newPos.x, newPos.y, cellItems[newPos.x, newPos.y]);
                if (cellItems[oldPos.x, oldPos.y] != null)
                {
                    var cellItemsAmin = cellItems[oldPos.x, oldPos.y].GetComponent<Animation>();
                    if (cellItemsAmin != null)
                    {
                        if (cellItemsAmin.GetClip(movingAnimationName) != null)
                            cellItemsAmin.RemoveClip(movingAnimationName);

                        var movingClip = GetMovingClip(newPos.x, newPos.y, oldPos.x, oldPos.y, Width, Height, moveDelay);
                        cellItemsAmin.AddClip(movingClip, movingAnimationName);
                        cellItemsAmin.Play(movingAnimationName);
                    }
                }

                if (cellItems[newPos.x, newPos.y] != null)
                {
                    var cellItemsAmin = cellItems[newPos.x, newPos.y].GetComponent<Animation>();
                    if (cellItemsAmin != null)
                    {
                        if (cellItemsAmin.GetClip(movingAnimationName) != null)
                            cellItemsAmin.RemoveClip(movingAnimationName);

                        var movingClip = GetMovingClip(oldPos.x, oldPos.y, newPos.x, newPos.y, Width, Height, moveDelay);
                        cellItemsAmin.AddClip(movingClip, movingAnimationName);
                        cellItemsAmin.Play(movingAnimationName);
                    }
                }
            }
            finally
            {
                Invoke("GameStepActionDone", moveDelay);
            }
        }));
    }

    void Match3Lib.IGameEventsListener.OnScore(int newScore, int gameStep)
    {
        AddGameStepAction(gameStep, new System.Action(() =>
        {
            try
            {
                scoreText.text = newScore.ToString();
            }
            finally
            {
                Invoke("GameStepActionDone", 0);
            }
        }));
    }

    void AddGameStepAction(int gameStep, System.Action targetAction)
    {
        if (!gameStepAction.ContainsKey(gameStep))
            gameStepAction.Add(gameStep, new List<System.Action>());

        gameStepAction[gameStep].Add(targetAction);

        if (gameStep == currentStep)
            currentStepMaxValue++;
    }

    void GameStepActionDone()
    {
        this.currentStepValue++;
    }

    void SetCellCallback(int x, int y, GameObject targetCellObj)
    {
        if (targetCellObj == null)
            return;

        var cellButton = targetCellObj.GetComponent<UnityEngine.UI.Button>();

        if (cellButton != null)
        {
            var cellPos = new Vector2Int(x, y);
            cellButton.onClick.RemoveAllListeners();
            cellButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { OnCellClick(cellPos.x, cellPos.y); }));
        }
    }

    static GameObject CopyCell(GameObject prefab, Transform parent, int x, int y, int width, int height)
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
    static void MoveToCell(int x, int y, int width, int height, ref RectTransform item)
    {
        var offsetMin = new Vector2(item.offsetMin.x, item.offsetMin.y);
        var offsetMax = new Vector2(item.offsetMax.x, item.offsetMax.y);

        item.anchorMin = new Vector2(x * 1.0f / width, y * 1.0f / height);
        item.anchorMax = new Vector2((x + 1) * 1.0f / width, (y + 1) * 1.0f / height);
        item.offsetMin = offsetMin;
        item.offsetMax = offsetMax;
    }

    static void MoveCellByAnim(int oldX, int oldY, int newX, int newY, int width, int height, float time, GameObject cellItem)
    {
        if (cellItem != null)
        {
            var cellItemAmin = cellItem.GetComponent<Animation>();
            if (cellItemAmin != null)
            {
                if (cellItemAmin.GetClip(movingAnimationName) != null)
                    cellItemAmin.RemoveClip(movingAnimationName);

                cellItemAmin.AddClip(GetMovingClip(oldX, oldY, newX, newY, width, height, time), movingAnimationName);
                cellItemAmin.Play(movingAnimationName);
            }
        }
    }

    static AnimationClip GetMovingClip(float startX, float startY, float endX, float endY, int width, int height, float time)
    {
        var result = new AnimationClip();
        result.legacy = true;
        result.SetCurve("", typeof(RectTransform), "m_AnchorMin.x", GetAnimationCurve(startX / width, endX / width, time));
        result.SetCurve("", typeof(RectTransform), "m_AnchorMin.y", GetAnimationCurve(startY / width, endY / width, time));
        result.SetCurve("", typeof(RectTransform), "m_AnchorMax.x", GetAnimationCurve((startX + 1) / width, (endX + 1) / width, time));
        result.SetCurve("", typeof(RectTransform), "m_AnchorMax.y", GetAnimationCurve((startY + 1) / width, (endY + 1) / width, time));
        result.name = movingAnimationName;

        return result;
    }

    static AnimationCurve GetAnimationCurve(float startValue, float endValue, float time)
    {
        var result = new AnimationCurve();
        result.AddKey(0, startValue);
        result.AddKey(time, endValue);

        return result;
    }
}