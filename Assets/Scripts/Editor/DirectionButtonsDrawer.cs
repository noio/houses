using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DirectionButtonsAttribute))]
public class DirectionButtonsDrawer : PropertyDrawer
{
    const float MinimumButtonWidth = 40.0f;

    int enumLength;
    float enumWidth;

    int buttonsPerRow;
    int numRows;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SetDimensions(property);
        return numRows * EditorGUIUtility.singleLineHeight + (numRows - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    void SetDimensions(SerializedProperty property) {
        enumLength = property.enumNames.Length;
        enumWidth = (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 40 );

        buttonsPerRow = 3; // Mathf.FloorToInt(enumWidth / MinimumButtonWidth);
        numRows = 3; //Mathf.CeilToInt((float)enumLength / (float)buttonsPerRow);
    }

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        SetDimensions(_property);

        int buttonsIntValue = 0;
        bool[] buttonPressed = new bool[enumLength];
        float buttonWidth = 30; //enumWidth / Mathf.Min(buttonsPerRow, enumLength);

        EditorGUI.LabelField(new Rect(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height), _label);

        EditorGUI.BeginChangeCheck ();

        for (int row = 0; row < numRows; row++) {
            for (int btn = 0; btn < buttonsPerRow; btn++) {
                int i = btn + row * buttonsPerRow;

                if (i >= enumLength) {
                    break;
                }

                // Check if the button is/was pressed
                if ((_property.intValue & (1 << i)) == 1 << i) {
                    buttonPressed[i] = true;
                }

                Rect buttonPos = new Rect(_position.x + EditorGUIUtility.labelWidth + buttonWidth * btn, _position.y + row * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), buttonWidth, EditorGUIUtility.singleLineHeight);
                buttonPressed[i] = GUI.Toggle(buttonPos, buttonPressed[i], _property.enumNames[i], EditorStyles.miniButton);

                if (buttonPressed[i])
                    buttonsIntValue += 1 << i;
            }
        }

        if (EditorGUI.EndChangeCheck()) {
            _property.intValue = buttonsIntValue;
        }
    }
}