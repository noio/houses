using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagButtonsAttribute))]
public class EnumFlagButtonsDrawer : PropertyDrawer
{
    const float MinimumButtonWidth = 100.0f;

    int enumLength;
    float enumWidth;

    int numBtns;
    int numRows;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SetDimensions(property);
        return numRows * EditorGUIUtility.singleLineHeight + (numRows - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    void SetDimensions(SerializedProperty property) {
        enumLength = property.enumNames.Length;
        enumWidth = (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 40 );

        numBtns = Mathf.FloorToInt(enumWidth / MinimumButtonWidth);
        numRows = Mathf.CeilToInt((float)enumLength / (float)numBtns);
    }

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        SetDimensions(_property);

        int buttonsIntValue = 0;
        bool[] buttonPressed = new bool[enumLength];
        float buttonWidth = enumWidth / Mathf.Min(numBtns, enumLength);

        EditorGUI.LabelField(new Rect(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height), _label);

        EditorGUI.BeginChangeCheck ();

        for (int row = 0; row < numRows; row++) {
            for (int btn = 0; btn < numBtns; btn++) {
                int i = btn + row * numBtns;

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