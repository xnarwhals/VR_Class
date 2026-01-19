using System.Reflection;
using UnityEngine;

public class TVDucks : DuckPuzzleBase
{
    [SerializeField] private Animator btnAnim;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override System.Collections.IEnumerator OnPuzzleCompleteSequence()
    {
        if (btnAnim != null) btnAnim.SetTrigger("Rise");
        
        yield return null;
    }
}
