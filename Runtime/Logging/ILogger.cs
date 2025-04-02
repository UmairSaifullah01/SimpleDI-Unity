using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	public interface ILogger
	{
		void Log(string        message, LogType logType = LogType.Log);
		void LogWarning(string message);
		void LogError(string   message);
	}


}