using UnityEngine;

public class UIDisplay : MonoBehaviour
{
    [Header("Ссылки на логику заданий")]
    public Task1_HydraulicPress t1;
    public Task2_FloatingBody   t2;
    public Task3_VariableDensity t3;   // добавим и третье, чтобы показывать ρ2

    [Header("Заголовки")]
    public string title = "Лабораторная работа 10";

    private GUIStyle labelStyle;
    private GUIStyle titleStyle;

    void InitStyles()
    {
        if (labelStyle != null) return;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;
        labelStyle.wordWrap = true;

        titleStyle = new GUIStyle(labelStyle);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
    }

    void OnGUI()
    {
        InitStyles();

        GUILayout.BeginArea(new Rect(10, 10, 720, 150), GUI.skin.box);

        GUILayout.Label(title, titleStyle);
        GUILayout.Space(4);

        GUILayout.Label("Задание 1: " +
            (t1 != null ? t1.GetResult() : "—"), labelStyle);

        GUILayout.Label("Задание 2: " +
            (t2 != null ? t2.GetResult() : "—"), labelStyle);

        GUILayout.Label("Задание 3: ρ2(t) — цвет тела меняется от cyan (лёгкое) " +
                        "к red (тяжёлое)", labelStyle);

        GUILayout.Space(4);
        GUILayout.Label("Клавиши 0 / 1 / 2 / 3 — переключение вида камеры", labelStyle);

        GUILayout.EndArea();
    }
}