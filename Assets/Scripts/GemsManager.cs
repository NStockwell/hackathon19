using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GemsManager : MonoBehaviour
{
    public Vector2 boardCellSize;
    public Vector2 boardCellPadding;
    public Vector2Int boardNumCells;
    public Vector2 boardPosition;

    public GameObject gemPrefab;
    public GemsInfoScriptableObject gemsInfo;

    public GameObject gemExplosion;
    public PowerupController powerupController;

    private Gem[,] gemsTable;
    private GameObject boardGameObject;

    private Gem firstSelectedGem;
    private Gem secondSelectedGem;

    private Gem gem1Animated;
    private Gem gem2Animated;
    private bool animatingSwap;
    private bool animatingFall;

    private bool isOnline;
    private bool isMyTurn;
    private Client gameClient;


    // Start is called before the first frame update
    void Start()
    {
        gameClient = FindObjectOfType<Client>();
        isOnline = gameClient != null;

        if(isOnline)
            GameManager.Instance.OnSceneLoaded(this);
        else
            StartGame(1337, true);
    }

    public void StartGame(int seed, bool isMyTurn)
    {
        if (gemPrefab == null || gemPrefab.GetComponent<Gem>() == null)
        {
            Debug.LogError("[GemsManager] Gem prefab is invalid. Shutting down...");
            this.enabled = false;
        }
        else if (gemsInfo == null || gemsInfo.Info == null || gemsInfo.Info.Length == 0)
        {
            Debug.LogError("[GemsManager] GemsInfoScriptableObject is invalid. Shutting down...");
            this.enabled = false;
        }
        else
        {
            CreateBoard(seed);
            this.isMyTurn = isMyTurn;
        }

        animatingSwap = false;
        animatingFall = false;
        gem1Animated = null;
        gem2Animated = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (gemsTable != null && isMyTurn && Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                //Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                if (hitInfo.transform.gameObject.tag == "Gem")
                {
                    //Debug.Log("It's working!");
                    Gem selectedGem = hitInfo.transform.gameObject.GetComponent<Gem>();
                    if (firstSelectedGem == null)
                    {
                        if (selectedGem.gemTypeSO.GemColor == GemColor.Empty || selectedGem.gemTypeSO.GemColor == GemColor.INVALID)
                        {
                            Debug.Log("Trying to select empty/invalid gem");
                        }
                        else
                        {
                            firstSelectedGem = selectedGem;
                        }
                    }
                    else
                    {
                        if (firstSelectedGem == selectedGem)
                        {
                            firstSelectedGem = null;
                        }
                        else
                        {
                            if (secondSelectedGem == null)
                            {
                                if (selectedGem.gemTypeSO.GemColor == GemColor.Empty || selectedGem.gemTypeSO.GemColor == GemColor.INVALID)
                                {
                                    Debug.Log("Trying to select empty/invalid gem");
                                }
                                else
                                {
                                    secondSelectedGem = selectedGem;
                                    Vector2Int diff = secondSelectedGem.GetIndex() - firstSelectedGem.GetIndex();
                                    if (Math.Abs(diff.magnitude - 1.0f) < Mathf.Epsilon)
                                    {
                                        MakeMove(firstSelectedGem, secondSelectedGem, true);
                                        if (gameClient != null)
                                        {
                                            gameClient.SendMakeMove(ref firstSelectedGem.index, ref secondSelectedGem.index);
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("Trying to swap gems too far apart");
                                        secondSelectedGem = null;
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("We shouldn't be here! Forgot to remove selection");
                                firstSelectedGem = secondSelectedGem = null;
                            }
                        }
                    }
                }
                else
                {
                    firstSelectedGem = null;
                    secondSelectedGem = null;
                }
            }
            else
            {
                firstSelectedGem = null;
                secondSelectedGem = null;
            }
        }
        if (animatingSwap)
        {
            if (gem1Animated.GetReachedTarget() && gem2Animated.GetReachedTarget())
            {
                animatingSwap = false;
                SwapGems(gem1Animated, gem2Animated);
                if (!animatingFall)
                {
                    gem1Animated = null;
                }
                gem2Animated = null;
            }
        }
        if (animatingFall)
        {
            if (gem1Animated == null)
            {
                animatingFall = false;
            }
            else
            {
                if (gem1Animated.GetReachedTarget())
                {
                    animatingFall = false;
                    gem1Animated = null;
                    BubbleSortEmptyCells();
                    RecheckBoard();
                }
            }
        }
    }

    public void CreateBoard(int seed)
    {
        if (boardGameObject != null)
        {
            Debug.LogWarning("Trying to create a new board, but this manager already has one. Ignoring...");
            return;
        }

        // Create the root node (aka board node)
        boardGameObject = new GameObject("Board");

        gemsTable = new Gem[boardNumCells.x, boardNumCells.y];
        System.Random rnd = new System.Random(seed);
        for (int x = 0, y = 0; x < boardNumCells.x; x++, y = 0)
        {
            for (; y < boardNumCells.y; y++)
            {
                GameObject obj = Instantiate(gemPrefab, GemPositionForIndex(x, y), Quaternion.identity, boardGameObject.transform) as GameObject;
                Gem gem = obj.GetComponent<Gem>();

                gem.SetType(gemsInfo.Info[rnd.Next(gemsInfo.Info.Length)]);

                gem.SetIndex(new Vector2Int(x, y));
                gemsTable[x, y] = gem;

                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                float scaleRatioX = boardCellSize.x / spriteRenderer.size.x;
                float scaleRatioY = boardCellSize.y / spriteRenderer.size.y;
                obj.transform.localScale = new Vector3(scaleRatioX, scaleRatioY, obj.transform.localScale.z);
            }
        }

        FixInitialBoard();
    }

    void FixInitialBoard()
    {
        for (int x = 0, y = 0; x < boardNumCells.x; x++, y = 0)
        {
            for (; y < boardNumCells.y; y++)
            {
                Gem currentGem = gemsTable[x, y];
                BackToBackCount cellCount = backToBackCountOnIndex(currentGem.GetIndex(), currentGem.gemTypeSO.GemColor);
                if (cellCount.HasEnoughToMatch())
                {
                    int currentIndex = 0;
                    GemColor[] neighboursColours = new GemColor[4];
                    if (x != boardNumCells.x - 1)
                    {
                        Vector2Int newIndex = currentGem.GetIndex() + Vector2Int.right;
                        neighboursColours[currentIndex++] = gemsTable[newIndex.x, newIndex.y].gemTypeSO.GemColor;
                    }
                    if (x != 0)
                    {
                        Vector2Int newIndex = currentGem.GetIndex() + Vector2Int.left;
                        neighboursColours[currentIndex++] = gemsTable[newIndex.x, newIndex.y].gemTypeSO.GemColor;
                    }
                    if (y != boardNumCells.y - 1)
                    {
                        Vector2Int newIndex = currentGem.GetIndex() + Vector2Int.up;
                        neighboursColours[currentIndex++] = gemsTable[newIndex.x, newIndex.y].gemTypeSO.GemColor;
                    }
                    if (y != 0)
                    {
                        Vector2Int newIndex = currentGem.GetIndex() + Vector2Int.down;
                        neighboursColours[currentIndex++] = gemsTable[newIndex.x, newIndex.y].gemTypeSO.GemColor;
                    }

                    GemColor gemColor = GemColor.Blue;

                    while (Array.Exists(neighboursColours, color => color == gemColor))
                    {
                        gemColor++;
                    }
                    currentGem.SetType(gemsInfo.Info[(int)gemColor - 1]);
                }
            }
        }
    }


    Vector3 GemPositionForIndex(int x, int y)
    {
        Vector3 halfSize = new Vector3((((boardNumCells.x - 1) * boardCellSize.x) + ((boardNumCells.x - 1) * boardCellPadding.x)) * 0.5f,
                                       (((boardNumCells.y - 1) * boardCellSize.y) + ((boardNumCells.y - 1) * boardCellPadding.y)) * 0.5f,
                                       0.0f);

        Vector3 finalPosition = new Vector3(boardPosition.x + x * (boardCellPadding.x + boardCellSize.x) - halfSize.x,
                                            boardPosition.y + y * (boardCellPadding.y + boardCellSize.y) - halfSize.y,
                                            0.0f);
        return finalPosition;
    }

    public void DeleteBoard()
    {
        if (boardGameObject == null)
        {
            Debug.LogWarning("Trying to delete the board, but there is no board. Ignoring...");
            return;
        }

        DestroyImmediate(boardGameObject);
        gemsTable = null;
    }

    int count = 0;
    GemColor color = GemColor.INVALID;

    // ----------------------
    public void SwapGems(Gem gem1, Gem gem2)
    {
        SwitchGemsPosition(gem1, gem2);
        BackToBackCount gem1Count;
        BackToBackCount gem2Count;
        if (GemsMatch(gem1, gem2, out gem1Count, out gem2Count))
        {
            Debug.Log("Gems Matched");
            ExplodeGems(gem1Count);
            ExplodeGems(gem2Count);
            
            DropGems();
            if(isOnline)
                isMyTurn = !isMyTurn;
        }
        else // reverse animation
        {
            Debug.Log("Reverting animation");
            SwitchGemsPosition(gem1, gem2);

            gem1Animated = gem1;
            gem2Animated = gem2;
            AnimateGemSwap(gem1Animated, gem2Animated, true);
        }

        firstSelectedGem = null;
        secondSelectedGem = null;
    }

    public bool GemsMatch(Gem gem1, Gem gem2, out BackToBackCount gem1Count, out BackToBackCount gem2Count)
    {
        gem1Count = backToBackCountOnIndex(gem1.GetIndex(), gem1.gemTypeSO.GemColor);
        gem2Count = backToBackCountOnIndex(gem2.GetIndex(), gem2.gemTypeSO.GemColor);
        //Debug.Log("CountG1H: " + gem1Count.horizontalGems.Count + " V: " + gem1Count.verticalGems.Count);
        //Debug.Log("CountG2H: " + gem2Count.horizontalGems.Count + " V: " + gem2Count.verticalGems.Count);
        return gem1Count.HasEnoughToMatch() || gem2Count.HasEnoughToMatch();
    }

    public void MakeMoveRemote(Vector2Int index1, Vector2Int index2)
    {
        Gem gem1 = gemsTable[index1.x, index1.y];
        Gem gem2 = gemsTable[index2.x, index2.y];
        MakeMove(gem1, gem2, true);
    }

    public void MakeMove(Gem gem1, Gem gem2, bool swapBoth)
    {
        gem1Animated = gem1;
        gem2Animated = gem2;
        AnimateGemSwap(gem1Animated, gem2Animated, swapBoth);

        animatingSwap = true;
    }

    public void AnimateGemSwap(Gem gem1, Gem gem2, bool swapBoth)
    {
        Vector3 gem1Transform = gem1.transform.position;
        Vector3 gem2Transform = gem2.transform.position;
        // Todo - animate gems moving
        gem1.SetTarget(gem2Transform);
        if (swapBoth)
        {
            gem2.SetTarget(gem1Transform);
        }
    }

    public void SwitchGemsPosition(Gem gem1, Gem gem2)
    {
        Vector2Int old1Index = new Vector2Int(gem1.GetIndex().x, gem1.GetIndex().y);

        gemsTable[gem1.GetIndex().x, gem1.GetIndex().y] = gem2;
        gemsTable[gem2.GetIndex().x, gem2.GetIndex().y] = gem1;
        gem1.index = gem2.GetIndex();
        gem2.index = old1Index;
    }

    public void ExplodeGems(BackToBackCount gemCount)
    {
        if (gemCount.HasEnoughHorizontalToMatch())
        {
            // TODO - collect type of gems to use as power
            foreach (Gem gem in gemCount.horizontalGems)
            {
                if (gem.gemTypeSO.GemType != GemType.Empty)
                {
                    Instantiate(gemExplosion, gem.transform.position, gem.transform.rotation);
                    count++;
                    if (color != gem.gemTypeSO.GemColor)
                        color = gem.gemTypeSO.GemColor;
                }
                GemTypeSO gemTypeSO = new GemTypeSO();
                gemTypeSO.GemType = GemType.Empty;
                gemTypeSO.GemColor = GemColor.Empty;
                gem.SetType(gemTypeSO);
            }
        }
        if (gemCount.HasEnoughVerticalToMatch())
        {
            // TODO - collect type of gems to use as power
            foreach (Gem gem in gemCount.verticalGems)
            {
                if (gem.gemTypeSO.GemType != GemType.Empty)
                {
                    Instantiate(gemExplosion, gem.transform.position, gem.transform.rotation);
                    count++;
                    if(color != gem.gemTypeSO.GemColor)
                        color = gem.gemTypeSO.GemColor;
                }
                GemTypeSO gemTypeSO = new GemTypeSO();
                gemTypeSO.GemType = GemType.Empty;
                gemTypeSO.GemColor = GemColor.Empty;
                gem.SetType(gemTypeSO);
            }
        }

        if(count > 0)
        {
            PlayerPowerups(count, color);
            count = 0;
        }
    }

    public void PlayerPowerups(int gemCount, GemColor gemColor)
    {
        Debug.Log(gemCount);
        Debug.Log(gemColor);
        powerupController.SetAnimation(gemColor, gemCount, !isMyTurn);
    }

    public void DropGems()
    {
        int maxEmptySlots = 0;
        for (int x = 0; x < boardNumCells.x; x++)
        {
            maxEmptySlots = Math.Max(DropColumn(x, maxEmptySlots), maxEmptySlots);
        }
    }

    public int DropColumn(int x, int maxEmptySlots)
    {
        for (int y = 0; y < boardNumCells.y; y++)
        {
            if (gemsTable[x, y].gemTypeSO.GemColor == GemColor.Empty)
            {
                for (int i = y + 1; i < boardNumCells.y; i++)
                {
                    if (gemsTable[x, i].gemTypeSO.GemColor == GemColor.Empty)
                    {
                        continue;
                    }
                    else
                    {
                        int numEmptySlots = i - y;
                        if (numEmptySlots > maxEmptySlots)
                        {
                            maxEmptySlots = numEmptySlots;
                        }
                        for (int j = y; j < boardNumCells.y - numEmptySlots; j++)
                        {
                            animatingFall = true;
                            if (numEmptySlots == maxEmptySlots)
                            {
                                gem1Animated = gemsTable[x, j + numEmptySlots];
                            }
                            AnimateGemSwap(gemsTable[x, j + numEmptySlots], gemsTable[x, j], false);
                        }
                        return numEmptySlots;
                    }
                }
            }
        }
        return 0;
    }

    public void BubbleSortEmptyCells()
    {
        for (int i = 0; i < boardNumCells.x; i++)
        {
            BubbleSortEmptyCells(i);
        }
    }

    public void RecheckBoard()
    {
        bool exploded = false;
        for(int x = 0; x < boardNumCells.x; x++)
        {
            for(int y = 0; y < boardNumCells.y; y++)
            {
                BackToBackCount count = backToBackCountOnIndex(new Vector2Int(x, y), gemsTable[x, y].gemTypeSO.GemColor);
                if(count.HasEnoughToMatch())
                {
                    ExplodeGems(count);
                    exploded = true;
                }
            }
        }
        if(exploded)
        {
            DropGems();
        }
    }

    public void BubbleSortEmptyCells(int x)
    {
        for (int y = boardNumCells.y - 2; y >= 0; y--)
        {
            if (gemsTable[x, y].gemTypeSO.GemColor == GemColor.Empty)
            {
                for (int i = y; i < boardNumCells.y - 1; i++)
                {
                    //Debug.Log("Swapping " + i + " with " + (i + 1));
                    SwitchGemsPosition(gemsTable[x, i], gemsTable[x, i + 1]);
                    gemsTable[x, i].transform.position = GemPositionForIndex(x, i);
                    gemsTable[x, i + 1].transform.position = GemPositionForIndex(x, i + 1);
                }
            }
        }
    }


    // -------------- BackToBackCountClass
    public class BackToBackCount
    {
        public BackToBackCount()
        {
            horizontalGems = new ArrayList();
            verticalGems = new ArrayList();
        }

        public ArrayList horizontalGems;
        public ArrayList verticalGems;

        public bool HasEnoughToMatch()
        {
            return HasEnoughVerticalToMatch() || HasEnoughHorizontalToMatch();
        }

        public bool HasEnoughHorizontalToMatch()
        {
            return horizontalGems.Count >= 3;
        }
        public bool HasEnoughVerticalToMatch()
        {
            return verticalGems.Count >= 3;
        }
    };

    public BackToBackCount backToBackCountOnIndex(Vector2Int index, GemColor gemColor)
    {
        BackToBackCount count = new BackToBackCount();

        Gem rootGem = gemsTable[index.x, index.y];
        if (rootGem != null)
        {
            count.horizontalGems.Add(rootGem);
            count.verticalGems.Add(rootGem);
        }

        // Count left
        for (int x = index.x - 1; x >= 0; x--)
        {
            Gem gem = gemsTable[x, index.y];
            if (gem == null || !gem.CanBeMatchedWith(gemColor)) break;
            else count.horizontalGems.Add(gem);
        }

        // Count right
        for (int x = index.x + 1; x < boardNumCells.x; x++)
        {
            Gem gem = gemsTable[x, index.y];
            if (gem == null || !gem.CanBeMatchedWith(gemColor)) break;
            else count.horizontalGems.Add(gem);
        }

        // Count up
        for (int y = index.y + 1; y < boardNumCells.y; y++)
        {
            Gem gem = gemsTable[index.x, y];
            if (gem == null || !gem.CanBeMatchedWith(gemColor)) break;
            else count.verticalGems.Add(gem);
        }

        // Count down
        for (int y = index.y - 1; y >= 0; y--)
        {
            Gem gem = gemsTable[index.x, y];
            if (gem == null || !gem.CanBeMatchedWith(gemColor)) break;
            else count.verticalGems.Add(gem);
        }

        return count;
    }

    // -----------------------
    [HideInInspector]
    public bool EditorIsDirty = false;
    void OnValidate()
    {
        boardCellSize = new Vector2(Mathf.Max(0.1f, boardCellSize.x), Mathf.Max(0.1f, boardCellSize.y));
        boardCellPadding = new Vector2(Mathf.Max(0.0f, boardCellPadding.x), Mathf.Max(0.0f, boardCellPadding.y));
        boardNumCells = new Vector2Int(Math.Max(1, boardNumCells.x), Math.Max(1, boardNumCells.y));

        EditorIsDirty = true;
    }
}
