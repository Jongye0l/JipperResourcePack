using System;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable InconsistentNaming

public class scrLogoText {
#if R141After
	public void ColorLogo(Color? col, bool isFire) { }
#else
	public void ColorLogo(Color col, bool isFire) { }
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
	public static void LoadScene(string name) { }
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
	public void CalculatePercentAcc() { }
#endif
}

#if R141After
public class scrMarginTracker {
	public int[] hitMarginsCount;
#if R148After
	public void CalculatePercentAcc(bool increaseRemainingPlayerHits = false) { }
#else // R148After
	public void CalculatePercentAcc() { }
#endif // R148After
}

public class PlanetarySystem {
	public double speed;
}

public class scrPlayer {
	public PlanetarySystem planetarySystem;
}

public class scrLoader {
	public void LoadScene(string name) { }
}


public class scrPlayerManager {
	public static scrPlayerManager instance => throw new Exception("stub");
	public static int playerCount;
	public scrMistakesManager mistakesManager;
}
#endif // R141After

#if R148After
public class Persistence {
	public static bool enableCompetitiveMode => throw new Exception("stub");
}
#endif
