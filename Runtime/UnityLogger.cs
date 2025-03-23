using UnityEngine;


namespace THEBADDEST.SimpleDependencyInjection
{


	public class UnityLogger : ILogger
	{
		public void Log(string message, LogType logType = LogType.Log)
		{
			switch (logType)
			{
				case LogType.Log:
					Debug.Log($"[SimpleDI] {message}");
					break;
				case LogType.Warning:
					Debug.LogWarning($"[SimpleDI] {message}");
					break;
				case LogType.Error:
					Debug.LogError($"[SimpleDI] {message}");
					break;
			}
		}

		public void LogWarning(string message) => Log(message, LogType.Warning);
		public void LogError(string   message) => Log(message, LogType.Error);
	}


}