using UnityEngine;


namespace THEBADDEST.UnityDI
{


	public class UnityLogger : ILogger
	{
		public void Log(string message, LogType logType = LogType.Log)
		{
			switch (logType)
			{
				case LogType.Log:
					Debug.Log($"[UnityDI] {message}");
					break;
				case LogType.Warning:
					Debug.LogWarning($"[UnityDI] {message}");
					break;
				case LogType.Error:
					Debug.LogError($"[UnityDI] {message}");
					break;
			}
		}

		public void LogWarning(string message) => Log(message, LogType.Warning);
		public void LogError(string   message) => Log(message, LogType.Error);
	}


}