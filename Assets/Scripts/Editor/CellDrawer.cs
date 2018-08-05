using Assets.Scripts.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// AUTHOR - Victor

[CustomPropertyDrawer(typeof(Cell))]
public class CellDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        EditorGUI.PropertyField(new Rect(position.x, position.y, 100, 40), property.FindPropertyRelative("State"), new GUIContent("State"));
        EditorGUI.PropertyField(new Rect(position.x, position.y, 200, 40), property.FindPropertyRelative("Elevation"), new GUIContent("Elevation"));

        EditorGUI.EndProperty();
    }
}

