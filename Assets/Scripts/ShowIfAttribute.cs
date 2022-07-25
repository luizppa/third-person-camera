using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ShowIfAttribute : PropertyAttribute
{
  public string[] Conditions { get; private set; }

  public ShowIfAttribute(params string[] conditions)
  {
    Conditions = conditions;
  }
}
