using System;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable InconsistentNaming

public class scrLogoText {
#if R141After
	public void ColorLogo(Color? col, bool isFire) => throw new Exception("stub");
#else
	public void ColorLogo(Color col, bool isFire) => throw new Exception("stub");
#endif
}

public class scrController {
	public static scrController instance => throw new Exception("stub");
#if R141After
	public scrPlayer playerOne => throw new Exception("stub");
#else
	public scrMistakesManager mistakesManager;
	public double speed;
#endif
}

public class ADOBase {
#if R141After
	public static scrLoader loader;
#else
	public static void LoadScene(string name) => throw new Exception("stub");
#endif
}

public class scrMistakesManager {
#if R141After
	public static scrMarginTracker[] marginTrackers;
	public float percentAcc => throw new Exception("stub");
	public float percentXAcc => throw new Exception("stub");
#else
	public static int[] hitMarginsCount;
	public float percentAcc;
	public float percentXAcc;
	public void CalculatePercentAcc() => throw new Exception("stub");
#endif
}

#if R141After
public class scrMarginTracker {
	public int[] hitMarginsCount;
	public void CalculatePercentAcc() => throw new Exception("stub");
#if R149After
	public float maxPossibleXAcc => throw new Exception("stub");
#endif // R149After
}

public class PlanetarySystem {
	public double speed;
}

public class scrPlayer {
	public PlanetarySystem planetarySystem;
}

public class scrLoader {
	public void LoadScene(string name) => throw new Exception("stub");
}


public class scrPlayerManager {
	public static scrPlayerManager instance => throw new Exception("stub");
	public static int playerCount;
	public scrMistakesManager mistakesManager;
}
#endif // R141After

#if R149After
public class Persistence {
	public static bool enableCompetitiveMode => throw new Exception("stub");
}
#endif

public static class RDUtils {
	public static float SetIfNaN(this float f, float value) => throw new Exception("stub");
}
