using UnityEngine;


namespace THEBADDEST.UnityDI
{


	public interface ILogger
	{
		void Log(string        message, LogType logType = LogType.Log);
		void LogWarning(string message);
		void LogError(string   message);
	}


}