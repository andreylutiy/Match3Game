using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameUiController : MonoBehaviour, Match3Lib.IGameEventsListener
{
    public RectTransform gamePanel;
    public RectTransform backgroundItemsPanel;
    public RectTransform gameItemsPanel;
    public RectTransform bangItemsPanel;
    public RectTransform selectedItemPointer;
    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text timeText;

    public GameObject[] cellItemPrefabs;
    public GameObject cellBangItemPrefab;
    public GameObject cellBackgroundPrefab;

    public float createDelay = 0.15f;
    public float moveDelay = 0.5f;
    public float destroyDelay = 0.3f;

    GameObject[] cellBackgrounds = new GameObject[0];
    GameObject[,] cellItems = new GameObject[0, 0];

    Vector2Int selectedItemPos = new Vector2Int(-1, -1);
    readonly Vector2Int selectedItemZeroPos = new Vector2Int(-1, -1);
    const string movingAnimationName = "MovingClip";

    System.Action<int, int, int, int> makeStepAction = new System.Action<int, int, int, int>((x0, y0, x1, y1) => { });

    int currentStep = -1;
    int currentStepValue = 0;
    int currentStepMaxValue = 0;
    Dictionary<int, List<System.Action>> gameStepAction = new Dictionary<int, List<System.Action>>();

    private void Update()
    {
        var isNextStep = gameStepAction.Keys.Where(item => item > currentStep).Count() > 0; ;// gameStepAction.ContainsKey(currentStep);
        if (gameStepAction.ContainsKey(currentStep))
        {
            isNextStep = isNextStep&& currentStepValue == currentStepMaxValue;
        }
        //else
        //{
        //    isNextStep = gameStepAction.Keys.Where(item => item > currentStep).Count() > 0;
        //}

        if (isNextStep)
        {
            gameStepAction.Remove(currentStep);
            currentStep++;
            currentStepValue = 0;
            currentStepMaxValue = gameStepAction.ContainsKey(currentStep) ? gameStepAction[currentStep].Count : 0;
        }
        if (gameStepAction.ContainsKey(currentStep))
        {
            var actualActions = gameStepAction[currentStep];
            if (actualActions.Count > 0)
            {
                lock (actualActions)
                {
                    for (int i = 0; i < actualActions.Count; i++)
                    {
                        actualActions[i]();
                    }
                    actualActions.Clear();
                }
            }

        }
    }

    public void StartGame(int width, int height, System.Action<int, int, int, int> makeStepAction)
    {
        selectedItemPointer.gameObject.SetActive(false);
        gamePanel.sizeDelta = new Vector2(gamePanel.rect.height * (width * 1.0f / height), gamePanel.sizeDelta.y);//, gamePanel.sizeDelta.y);
        createDelay = createDelay/(0.5f*(width+height));
        this.makeStepAction = makeStepAction;
        for (int i = 0; i < cellBackgrounds.Length; i++)
        {
            Destroy(cellBackgrounds[i]);
        }
        for (int i = 0; i < cellItems.GetLength(0); i++)
        {
            for (int j = 0; j < cellItems.GetLength(1); j++)
            {
                Destroy(cellItems[i, j]);
            }
        }
        cellBackgrounds = new GameObject[width * height];
        cellItems = new GameObject[width, height];

        var prefabTransform = cellBackgroundPrefab.GetComponent<RectTransform>();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var currentCell = CopyCell(cellBackgroundPrefab, backgroundItemsPanel, i, j, width, height);
                cellBackgrounds[i * width + j] = currentCell;
            }
        }
        MoveToCell(0, 0, width, height, ref selectedItemPointer);
        
    }

    public void OnCellClick(int x, int y)
    {
        if (selectedItemPos == selectedItemZeroPos)
        {
            selectedItemPos = new Vector2Int(x, y);
            selectedItemPointer.gameObject.SetActive(true);
            MoveToCell(x, y, cellItems.GetLength(0), cellItems.GetLength(1), ref selectedItemPointer);
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
    void Match3Lib.IGameEventsListener.OnCellCreate(int x, int y, int cellType, int gameStep)
    {
        AddGameStepAction(gameStep, new System.Action(() =>
        {
            var movingDelay = createDelay*(cellItems.GetLength(1) + y);
            try
            {
                cellItems[x, y] = CopyCell(cellItemPrefabs[cellType], gameItemsPanel, x, y, cellItems.GetLength(0), cellItems.GetLength(1));
                SetCellCallback(x, y, cellItems[x, y]);
                MoveCellByAnim(x, cellItems.GetLength(1)+y, x, y, cellItems.GetLength(0), cellItems.GetLength(1), movingDelay, cellItems[x, y]);
                //UnityEngine.Debug.Log("OnCellCreate " + x+";"+y);
            }
            finally
            {
                Invoke("GameStepActionDone", movingDelay);
            }
        }));
    }
    void Match3Lib.IGameEventsListener.OnCellDestroy(int x, int y, int gameStep)
    {
        AddGameStepAction(gameStep, new System.Action(() =>
        {
            try
            {
                var bangItem = Instantiate(cellBangItemPrefab);
                var bangItemTransform = bangItem.GetComponent<RectTransform>();
                bangItemTransform.parent = bangItemsPanel;
                bangItemTransform.offsetMin = cellItems[x, y].GetComponent<RectTransform>().offsetMin;
                bangItemTransform.offsetMax = cellItems[x, y].GetComponent<RectTransform>().offsetMax;
                MoveToCell(x, y, cellItems.GetLength(0), cellItems.GetLength(1), ref bangItemTransform);
                Destroy(cellItems[x, y]);
            }
            finally
            {
                Invoke("GameStepActionDone", destroyDelay);
            }
        }));
    }
    void Match3Lib.IGameEventsListener.OnCellMove(int oldX, int oldY, int newX, int newY, int gameStep)
    {
        AddGameStepAction(gameStep, new System.Action(() =>
        {
            try
            {
                var tempCell = cellItems[oldX, oldY];
                cellItems[oldX, oldY] = cellItems[newX, newY];
                cellItems[newX, newY] = tempCell;
                SetCellCallback(oldX, oldY, cellItems[oldX, oldY]);
                SetCellCallback(newX, newY, cellItems[newX, newY]);
                if (cellItems[oldX, oldY] != null)
                {
                    //var cellItemsTransform = cellItems[oldX, oldY].GetComponent<RectTransform>();
                    var cellItemsAmin = cellItems[oldX, oldY].GetComponent<Animation>();
                    if (//cellItemsTransform != null&&
                    cellItemsAmin != null)
                    {
                        if (cellItemsAmin.GetClip(movingAnimationName)!= null)
                        {
                            cellItemsAmin.RemoveClip(movingAnimationName);
                        }
                        cellItemsAmin.AddClip( GetMovingClip(newX, newY, oldX, oldY, cellItems.GetLength(0), cellItems.GetLength(1), moveDelay), movingAnimationName) ;
                        cellItemsAmin.Play(movingAnimationName);
                        //    MoveToCell(oldX, oldY, cellItems.GetLength(0), cellItems.GetLength(1), ref cellItemsTransform);
                    }
                }
                if (cellItems[newX, newY] != null)
                {
                    var cellItemsAmin = cellItems[newX, newY].GetComponent<Animation>();
                    if ( cellItemsAmin != null)
                    {
                        if (cellItemsAmin.GetClip(movingAnimationName) != null)
                        {
                            cellItemsAmin.RemoveClip(movingAnimationName);
                        }
                        cellItemsAmin.AddClip(GetMovingClip( oldX, oldY,newX, newY, cellItems.GetLength(0), cellItems.GetLength(1), moveDelay), movingAnimationName);
                        cellItemsAmin.Play(movingAnimationName);
                    }
                    //var cellItemsTransform = cellItems[newX, newY].GetComponent<RectTransform>();
                    //if (cellItemsTransform != null)
                    //{
                    //    MoveToCell(newX, newY, cellItems.GetLength(0), cellItems.GetLength(1), ref cellItemsTransform);
                    //}
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
        {
            gameStepAction.Add(gameStep, new List<System.Action>());
        }
        gameStepAction[gameStep].Add(targetAction);
        if (gameStep == currentStep)
        {
            currentStepMaxValue++;
        }
    }

    void GameStepActionDone()
    {
        this.currentStepValue++;
    }
    void SetCellCallback(int x, int y, GameObject targetCellObj)
    {
        if (targetCellObj == null)
        {
            return;
        }
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
            resultRect.offsetMin = prefabRect.offsetMin; ;// sizeDelta = sizeDelta;
            resultRect.offsetMax = prefabRect.offsetMax; ;// sizeDelta = sizeDelta;
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
        item.offsetMin = offsetMin;// prefabTransform.offsetMin; ;// sizeDelta = sizeDelta;
        item.offsetMax = offsetMax;// prefabTransform.offsetMax; ;// sizeDelta = sizeDelta;
    }


    static void MoveCellByAnim(int oldX, int oldY, int newX, int newY, int width, int height, float time, GameObject cellItem)
    {
        if (cellItem != null)
        {
            var cellItemAmin = cellItem.GetComponent<Animation>();
            if(cellItemAmin != null)
            {
                if (cellItemAmin.GetClip(movingAnimationName) != null)
                {
                    cellItemAmin.RemoveClip(movingAnimationName);
                }
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