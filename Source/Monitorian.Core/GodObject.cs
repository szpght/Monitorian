using Microsoft.Win32;
using Monitorian.Core.Models.Monitor;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Monitorian.Core;
internal static class GodObject
{
	private readonly static Stopwatch stopwatch = Stopwatch.StartNew();
	private readonly static Guid sessionId = Guid.NewGuid();
	private readonly static RegistryKey registry = Registry.CurrentUser.CreateSubKey("SOFTWARE\\szpght", true);
	private readonly static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MonitorCapability.SerializationHelper));

	public static MonitorCapability GetByDeviceInstanceIdOrDefault(string deviceInstanceId)
	{
		var existing = (byte[])registry.GetValue(deviceInstanceId);
		if (existing == null)
		{
			Log($"Cache miss for: {deviceInstanceId}");
			return null;
		}

		using var stream = new MemoryStream(existing);
		var helper = (MonitorCapability.SerializationHelper)serializer.ReadObject(stream);
		var sessionInfo = sessionId == helper.Session ? "current" : "previous";
		Log($"Cache hit from {sessionInfo} session for: {deviceInstanceId}");

		return new MonitorCapability(helper);
	}

	public static void Add(string deviceInstanceId, MonitorCapability capability)
	{
		Log($"Cache add for: {deviceInstanceId}");
		var helper = capability.ToHelper();
		helper.Session = sessionId;
		using var stream = new MemoryStream();
		serializer.WriteObject(stream, helper);
		registry.SetValue(deviceInstanceId, stream.ToArray());
	}

	public static void Log(string s) => Console.WriteLine($"{stopwatch.Elapsed}  {s}");
}
