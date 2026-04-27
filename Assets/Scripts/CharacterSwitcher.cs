using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitcher : MonoBehaviour
{
    public GameObject[] characters; // Закинем сюда всех наших игроков
    private int currentIndex = 0;

    void Update()
    {
        if (Keyboard.current == null) return;

        // Проверяем нажатие клавиш 1, 2, 3, 4
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchTo(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchTo(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchTo(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchTo(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) SwitchTo(4);
    }

    void SwitchTo(int index)
    {
        if (index >= characters.Length || characters[index] == null) return;

        // Запоминаем позицию текущего персонажа
        Vector3 lastPos = characters[currentIndex].transform.position;
        Quaternion lastRot = characters[currentIndex].transform.rotation;

        // Выключаем старого
        characters[currentIndex].SetActive(false);

        // Включаем нового
        currentIndex = index;
        characters[currentIndex].SetActive(true);

        // Переносим нового на место старого
        characters[currentIndex].transform.position = lastPos;
        characters[currentIndex].transform.rotation = lastRot;

        Debug.Log("Переключено на персонажа №" + (index + 1));
    }
}