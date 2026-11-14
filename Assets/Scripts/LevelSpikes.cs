using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Puts spikes into each level when it loads (called by LevelThemeManager).
// Touching a spike sends the player back to the last checkpoint.
// More spikes appear in later levels:
//   Level 1   - one spike on the ground before the door (to learn what they do)
//   Level 2+  - spikes on the ground under the gaps between platforms
//   Level 4+  - spikes in the middle of some platforms (jump over them!)
// Spikes are never put where you take off or land, or next to the switch, bounce pads or checkpoint.
public static class LevelSpikes
{
    private static Sprite spike;

    public static void Place(int level, GameObject ground, Material mat, float deadlyFloorRight)
    {
        if (spike == null) spike = Resources.Load<Sprite>("Spike");
        if (spike == null || ground == null) return;

        Vector3 gp = ground.transform.position;
        Vector3 gs = ground.transform.lossyScale;
        float groundTop = gp.y + gs.y * 0.5f;
        bool deadlyFloor = !float.IsNaN(deadlyFloorRight);

        GameObject door = GameObject.Find("Door");
        GameObject ship = GameObject.Find("Spaceship");
        float goalX = door != null ? door.transform.position.x : ship != null ? ship.transform.position.x : float.NaN;

        // Still platforms on the way to the goal, left to right.
        List<SpriteRenderer> platforms = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude)
            .Where(sr => sr.gameObject.name.StartsWith("Platform") &&
                         sr.GetComponentInParent<MovingPlatform>() == null &&
                         (float.IsNaN(goalX) || sr.bounds.center.x < goalX))
            .OrderBy(sr => sr.bounds.min.x)
            .ToList();
        if (platforms.Count == 0) return;

        var parent = new GameObject("Spikes").transform;

        // 1) Ground spikes between the last platform and the door.
        float lastRight = platforms.Max(p => p.bounds.max.x);
        if (!float.IsNaN(goalX))
        {
            float from = Mathf.Max(lastRight, deadlyFloor ? deadlyFloorRight : lastRight) + 1.2f;
            float to = goalX - 1.5f;
            if (to - from > 1.5f)
                Row(parent, mat, (from + to) * 0.5f, groundTop, level >= 6 ? 2 : 1);
        }

        // 2) Ground spikes under the gaps (not on lava / toxic levels - the floor is already deadly).
        if (level >= 2 && !deadlyFloor)
        {
            int wanted = Mathf.Min(1 + (level - 2) / 3, 3);
            for (int i = 0; i + 1 < platforms.Count && wanted > 0; i++)
            {
                float a = platforms[i].bounds.max.x, b = platforms[i + 1].bounds.min.x;
                if (b - a < 1.5f) continue;
                float x = (a + b) * 0.5f;
                if (Blocked(x, groundTop, 1.5f)) continue;
                Row(parent, mat, x, groundTop, 2);
                wanted--;
            }
        }

        // 3) A spike in the middle of some wide platforms (never the first one).
        if (level >= 4)
        {
            int wanted = Mathf.Min(1 + (level - 4) / 2, 3);
            var picks = platforms.Skip(1)
                .Where(p => p.bounds.size.x >= 3.5f)
                .Where(p => !Blocked(p.bounds.center.x, p.bounds.max.y, p.bounds.extents.x + 0.3f))
                .OrderByDescending(p => p.bounds.size.x)
                .Take(wanted);
            foreach (SpriteRenderer p in picks)
                Row(parent, mat, p.bounds.center.x, p.bounds.max.y, 1);
        }
    }

    // Is something important (switch, bounce pad, checkpoint, patrolling hazard, a low crystal)
    // sitting on this surface near x?
    private static bool Blocked(float x, float surfaceY, float halfWidth)
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            string n = t.gameObject.name;
            bool important = n.StartsWith("Switch") || n.StartsWith("BouncePad") || n.Contains("Checkpoint") ||
                             n.StartsWith("Patrol") || n.StartsWith("Crystal") || n.StartsWith("Door") ||
                             n.StartsWith("Spaceship") || t.CompareTag("Player");
            if (!important) continue;

            Vector3 p = t.position;
            float above = p.y - surfaceY;
            float reach = n.StartsWith("Crystal") ? 1.2f : 2.5f; // crystals higher up can be grabbed by jumping over
            if (Mathf.Abs(p.x - x) <= halfWidth && above > -0.5f && above < reach)
                return true;
        }
        return false;
    }

    private static void Row(Transform parent, Material mat, float centerX, float surfaceY, int count)
    {
        Vector2 size = spike.bounds.size;
        float startX = centerX - (count - 1) * size.x * 0.5f;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Spike");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(startX + i * size.x, surfaceY + size.y * 0.5f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spike;
            if (mat != null) sr.sharedMaterial = mat;
            sr.sortingOrder = 3;

            // A bit smaller than the picture, so a near-miss doesn't count as a hit.
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(size.x * 0.6f, size.y * 0.6f);
            box.offset = new Vector2(0f, -size.y * 0.15f);

            go.AddComponent<MovingHazard>(); // no patrol points = stays still, resets the player on touch
        }
    }
}
