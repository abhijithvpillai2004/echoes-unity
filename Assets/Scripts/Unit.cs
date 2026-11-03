using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool isEcho = false;
    [SerializeField] private Vector2Int startGridPosition;
    [SerializeField] private Vector2Int gridPosition;

    public Vector2Int StartGridPosition { get => startGridPosition; set => startGridPosition = value; }
    public Vector2Int GridPosition => gridPosition;

    private MageAnimation mageAnimation;

    private void Awake()
    {
        gridPosition = startGridPosition;
        EnsureAnimationComponent();
    }

    private void EnsureAnimationComponent()
    {
        if (mageAnimation == null)
        {
            mageAnimation = GetComponent<MageAnimation>();
            if (mageAnimation == null) mageAnimation = gameObject.AddComponent<MageAnimation>();
        }
    }

    public void SetStartGridPosition(Vector2Int newStart)
    {
        startGridPosition = newStart;
        gridPosition = newStart;
        SnapToGrid(gridPosition);
    }

    public void ResetUnit()
    {
        gridPosition = startGridPosition;
        SnapToGrid(gridPosition);
    }

    public void SetGridPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
        SnapToGrid(gridPosition);
    }

    public void MoveToGridPosition(Vector2Int newPos, Vector2Int dir)
    {
        Vector2Int oldPos = gridPosition;
        gridPosition = newPos;

        if (GridManager.Instance != null)
        {
            Vector3 startWorld = GridManager.Instance.GridToWorld(oldPos);
            Vector3 targetWorld = GridManager.Instance.GridToWorld(newPos);

            EnsureAnimationComponent();
            if (mageAnimation != null)
            {
                mageAnimation.PlayHop(startWorld, targetWorld, dir);
            }
            else
            {
                transform.position = targetWorld;
            }
        }
    }

    public void TriggerWallBump(Vector2Int dir)
    {
        if (isEcho) return;

        EnsureAnimationComponent();
        if (mageAnimation != null)
        {
            mageAnimation.PlayWallBump(dir);
        }
    }

    public void SnapToGrid()
    {
        SnapToGrid(gridPosition);
    }

    public void SnapToGrid(Vector2Int targetPos)
    {
        gridPosition = targetPos;
        if (GridManager.Instance != null)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPosition);
            EnsureAnimationComponent();
            if (mageAnimation != null)
            {
                mageAnimation.SnapToBase(worldPos);
            }
            else
            {
                transform.position = worldPos;
            }
        }
    }
}
