using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;


public class PlayerCoins : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    int newValue;

    IEnumerator myCouroutine;

    public void StartCoinAnimation()
    {
        //Debug.Log(GameManager.Instance.coinsEarnedInCurrentLevel + GameManager.Instance.playerCoins);
        myCouroutine = CoinIncrement(0.1f, GameManager.Instance.coinsEarnedInCurrentLevel + GameManager.Instance.playerCoins);
        StartCoroutine(myCouroutine);
    }

    IEnumerator CoinIncrement(float seconds, int valueTarget)
    {
        do
        {
            newValue = (int.Parse(coinText.text) + 1);
            coinText.text = newValue.ToString();

            yield return new WaitForSeconds(seconds);
        }
        while (newValue < valueTarget);

        GameManager.Instance.UpdateCoins(valueTarget);
    }
}
