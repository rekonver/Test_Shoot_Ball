using UnityEngine;

public static class AnimatorUtils
{
    public static float GetClipLength(Animator animator, string clipName)
    {
        if (animator == null || string.IsNullOrEmpty(clipName)) return -1f;

        var rac = animator.runtimeAnimatorController;
        if (rac == null) return -1f;

        var clips = rac.animationClips;
        foreach (var clip in clips)
        {
            if (clip == null) continue;
            if (clip.name == clipName || clip.name.Contains(clipName))
            {
                return clip.length;
            }
        }

        return -1f;
    }
}
