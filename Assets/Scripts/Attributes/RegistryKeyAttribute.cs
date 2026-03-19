using UnityEngine;

public class RegistryKeyAttribute : PropertyAttribute
{
    public string ListName { get; }

    public RegistryKeyAttribute(string listName)
    {
        ListName = listName;
    }
}