using UnityEngine;

namespace r.e.p.o_cheat;

public class PatchDelay : MonoBehaviour
{
	private void Start()
	{
		((MonoBehaviour)this).StartCoroutine(Loader.DelayedPatchRoutine());
	}
}
