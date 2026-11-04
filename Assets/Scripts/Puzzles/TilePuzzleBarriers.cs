using UnityEngine;

public class TilePuzzleBarriers : MonoBehaviour
{
    [SerializeField] private FillGridPuzzle puzzle;
    public void Configure(FillGridPuzzle value) => puzzle = value;
    private void LateUpdate()
    {
        if (puzzle != null && puzzle.IsSolved) gameObject.SetActive(false);
    }
}
