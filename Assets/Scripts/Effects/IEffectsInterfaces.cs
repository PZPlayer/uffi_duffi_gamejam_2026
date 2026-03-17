using Jam.Effects;
using System;
using UnityEngine;

public interface IPassive
{
    void OnPassiveUpdate(); // если надо пасивно раз в тик вызывать метод
}

public interface IActive //если надо активировать этот эффект с помощью клавиши -> в нашем случае R 
{
    void OnActiveCall();
}

public interface ICallable //если надо на что подписаться. Пример: Прыжок(Эвент) -> Выпускать ядовитый газ
{
    void Subsribe();
    void UnSubsribe();
}