using System;
using UnityEngine;

// Generates the game's sound effects and background music in code, so the game has
// sound even with no audio files. Drop your own clips into SoundManager / MusicManager
// in the Inspector and those are used instead.
public static class ProceduralAudio
{
    const int Rate = 44100;
    const float TwoPi = Mathf.PI * 2f;

    // f(t) gives the sample (-1..1) at time t in seconds.
    static AudioClip Make(string name, float seconds, Func<float, float> f)
    {
        int count = Mathf.CeilToInt(seconds * Rate);
        var data = new float[count];
        for (int i = 0; i < count; i++)
            data[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);

        AudioClip clip = AudioClip.Create(name, count, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Phase of a sine that slides from f0 to f1 over d seconds.
    static float Sweep(float t, float d, float f0, float f1)
    {
        return TwoPi * (f0 * t + (f1 - f0) / (2f * d) * t * t);
    }

    public static AudioClip Jump()
    {
        const float d = 0.18f;
        return Make("Jump", d, t => Mathf.Sin(Sweep(t, d, 300f, 760f)) * Mathf.Pow(1f - t / d, 2f) * 0.45f);
    }

    public static AudioClip Bounce()
    {
        const float d = 0.35f;
        return Make("Bounce", d, t =>
        {
            float p = Sweep(t, d, 180f, 560f);
            return (Mathf.Sin(p) + 0.3f * Mathf.Sin(2f * p)) * Mathf.Exp(-6f * t) * 0.5f;
        });
    }

    public static AudioClip Crystal()
    {
        return Make("Crystal", 0.35f, t =>
        {
            float f = t < 0.09f ? 880f : 1318.5f;
            float local = t < 0.09f ? t : t - 0.09f;
            return Mathf.Sin(TwoPi * f * t) * Mathf.Exp(-9f * local) * 0.45f;
        });
    }

    public static AudioClip Switch()
    {
        var rng = new System.Random(7);
        return Make("Switch", 0.22f, t =>
        {
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-70f * t) * 0.5f;
            float thump = Mathf.Sin(TwoPi * 140f * t) * Mathf.Exp(-16f * t) * 0.6f;
            return noise + thump;
        });
    }

    public static AudioClip Damage()
    {
        const float d = 0.4f;
        return Make("Damage", d, t => Mathf.Sign(Mathf.Sin(Sweep(t, d, 420f, 90f))) * (1f - t / d) * 0.25f);
    }

    public static AudioClip Win()
    {
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 783.99f, 1046.5f };
        const float step = 0.13f;
        return Make("Win", 0.95f, t =>
        {
            int i = Mathf.Min((int)(t / step), notes.Length - 1);
            float local = t - i * step;
            float tri = (2f / Mathf.PI) * Mathf.Asin(Mathf.Sin(TwoPi * notes[i] * t));
            float env = Mathf.Min(1f, local * 200f) * (i == notes.Length - 1 ? Mathf.Exp(-3f * local) : Mathf.Exp(-5f * local));
            return tri * env * 0.4f;
        });
    }

    static float Midi(int note)
    {
        return 440f * Mathf.Pow(2f, (note - 69) / 12f);
    }

    // A calm, looping space-y arpeggio (Am - F - C - G).
    public static AudioClip Music()
    {
        int[][] arp =
        {
            new[] { 69, 72, 76, 81 }, // Am
            new[] { 65, 69, 72, 77 }, // F
            new[] { 67, 72, 76, 79 }, // C
            new[] { 67, 71, 74, 79 }  // G
        };
        int[] bass = { 45, 41, 48, 43 };
        int[] pattern = { 0, 1, 2, 3, 2, 1, 2, 1 };

        const float noteLen = 0.3f;           // eighth note
        const float barLen = noteLen * 8f;    // 2.4 seconds
        float total = barLen * arp.Length;

        return Make("Music", total, t =>
        {
            int bar = Mathf.Min((int)(t / barLen), arp.Length - 1);
            float barT = t - bar * barLen;
            int step = Mathf.Min((int)(barT / noteLen), 7);
            float local = barT - step * noteLen;

            float lead = Mathf.Sin(TwoPi * Midi(arp[bar][pattern[step]]) * t);
            lead += 0.25f * Mathf.Sin(TwoPi * 2f * Midi(arp[bar][pattern[step]]) * t);
            lead *= Mathf.Min(1f, local * 300f) * Mathf.Exp(-5f * local) * 0.28f;

            float bf = Midi(bass[bar]);
            float tri = (2f / Mathf.PI) * Mathf.Asin(Mathf.Sin(TwoPi * bf * t));
            float bassEnv = Mathf.Min(1f, barT * 20f) * Mathf.Min(1f, (barLen - barT) * 20f);
            float bassSound = tri * bassEnv * 0.22f;

            float pad = Mathf.Sin(TwoPi * Midi(bass[bar] + 19) * t) * 0.05f * bassEnv;

            return lead + bassSound + pad;
        });
    }
}
