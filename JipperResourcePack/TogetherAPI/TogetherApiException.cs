using System;

namespace JipperResourcePack.TogetherAPI;

public class TogetherApiException(string message) : Exception(message);