namespace UsingCheck.Helpers;

class UsingCount
{
	public UsingCount(string usingName)
	{
		UsingName = usingName;
		Count = 1;
	}

	public string UsingName { get; }
	public int Count { get; set; }
}