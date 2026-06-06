using UnityEngine;

// Put this on the boss (alongside Health). The boss is invincible until every
// object tagged minionTag has been destroyed.
public class MinionGuard : MonoBehaviour
{
    [SerializeField] string minionTag = "Minion";

    public bool IsInvincible { get; private set; } = true;

    void Update()
    {
        if (!IsInvincible) return;
        if (GameObject.FindGameObjectsWithTag(minionTag).Length == 0)
            IsInvincible = false;
    }
}