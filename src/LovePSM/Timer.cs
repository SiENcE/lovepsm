/* PlayStation(R)Mobile SDK 1.21.00
 * Copyright (C) 2013 Sony Computer Entertainment Inc.
 * All Rights Reserved.
 */
using System;
using System.Threading;
using System.Diagnostics;

namespace LovePSM
{

/**
 * SampleTimer class
 */
public static class Timer
{
	//  Init and term

	public static bool Init()
	{
		stopwatch = new Stopwatch() ;
		stopwatch.Start() ;
		FrameCount = -1 ;
		return true ;
	}

	public static void Term()
	{
	}

	//  Frame trigger

	public static void StartFrame()
	{
		var currStartTick = stopwatch.ElapsedTicks ;
		FrameCount ++ ;
		if ( FrameCount != 0 ) {
			DeltaTime = (float)( currStartTick - prevStartTick ) / (float)Stopwatch.Frequency ;
			FrameRate = ( DeltaTime == 0.0f ) ? 0.0f : 1.0f / DeltaTime ;
			FrameTime = (float)( prevEndTick - prevStartTick ) / (float)Stopwatch.Frequency ;
			averageSampleFrame ++ ;
			averageSampleTime += FrameTime ;
		}
		prevStartTick = currStartTick ;

		IsAverageUpdated = false ;
		var averageSampleTick = currStartTick - averageSampleStart ;
		if ( FrameCount == 1 || averageSampleTick > Stopwatch.Frequency ) {
			if ( averageSampleFrame == 0 ) averageSampleFrame = 1 ;
			AverageFrameRate = (float)averageSampleFrame / averageSampleTick * Stopwatch.Frequency ;
			AverageFrameTime = averageSampleTime / averageSampleFrame ;
			IsAverageUpdated = true ;
			averageSampleStart = currStartTick ;
			averageSampleFrame = 0 ;
			averageSampleTime = 0.0f ;
		}
	}
	public static void EndFrame()
	{
		prevEndTick = stopwatch.ElapsedTicks ;
	}

	//  Stopwatch properties

	public static TimeSpan Elapsed {
		get { return stopwatch.Elapsed ; }
	}
	public static long ElapsedMilliseconds {
		get { return stopwatch.ElapsedMilliseconds ; }
	}
	public static long ElapsedMicroseconds {
		get { return stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency ; }
	}
	public static long ElapsedTicks {
		get { return stopwatch.ElapsedTicks ; }
	}
	public static long Frequency {
		get { return Stopwatch.Frequency ; }
	}

	//  Time values

	public static int FrameCount ;
	public static float DeltaTime ;
	public static float FrameRate ;
	public static float FrameTime ;
	public static float AverageFrameRate ;
	public static float AverageFrameTime ;
	public static bool IsAverageUpdated ;

	//  Variables

	static Stopwatch stopwatch ;
	static long prevStartTick ;
	static long prevEndTick ;

	static long averageSampleStart ;
	static int averageSampleFrame ;
	static float averageSampleTime ;
}

} // end ns Sample
