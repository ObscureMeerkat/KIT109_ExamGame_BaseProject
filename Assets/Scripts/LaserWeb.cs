using UnityEngine;
using System.Collections.Generic;

// One per Laser Web level. Stuck laser shots register as nodes. Every pair of nodes
// is joined by a laser. A laser is cut short where terrain blocks it (can't penetrate),
// and a laser that reaches the enemy damages it.
public class LaserWeb : MonoBehaviour
{
    [SerializeField] LineRenderer laserPrefab;   // a LineRenderer (2 points, glowing material)
    [SerializeField] LayerMask terrainMask;
    [SerializeField] LayerMask enemyMask;

    readonly List<Transform> nodes = new();
    readonly List<LineRenderer> lasers = new();

    public void RegisterNode(Transform node)
    {
        if (node != null && !nodes.Contains(node)) nodes.Add(node);
    }

    void LateUpdate()
    {
        nodes.RemoveAll(n => n == null);

        int laserIndex = 0;
        Collider2D enemyToDamage = null;

        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                Vector2 a = nodes[i].position;
                Vector2 b = nodes[j].position;
                Vector2 end = b;

                // First thing the line meets (node colliders are on a different layer,
                // so they're ignored).
                RaycastHit2D hit = Physics2D.Linecast(a, b, terrainMask | enemyMask);
                if (hit.collider != null)
                {
                    bool isEnemy = (enemyMask.value & (1 << hit.collider.gameObject.layer)) != 0;
                    if (isEnemy) enemyToDamage = hit.collider;   // laser reaches the enemy
                    else end = hit.point;                        // terrain: stop at the wall
                }

                LineRenderer lr = GetLaser(laserIndex++);
                lr.enabled = true;
                lr.positionCount = 2;
                lr.SetPosition(0, a);
                lr.SetPosition(1, end);
            }
        }

        for (int k = laserIndex; k < lasers.Count; k++)
            lasers[k].enabled = false;

        if (enemyToDamage != null)
        {
            Health h = enemyToDamage.GetComponentInParent<Health>();
            if (h != null) h.TakeDamage(1);
        }
    }

    LineRenderer GetLaser(int index)
    {
        while (lasers.Count <= index)
            lasers.Add(Instantiate(laserPrefab, transform));
        return lasers[index];
    }
}