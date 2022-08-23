using ManagerLib;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleResultController : MonoBehaviour {
    [SerializeField] private TMP_Text resultDisplay;
    [SerializeField] private Button homeBtn;
    [SerializeField] private Button lobbyBtn;
    // Start is called before the first frame update
    private void Start() {
        DisplayResult();
        homeBtn.onClick.AddListener(ReturnHome);
        lobbyBtn.onClick.AddListener(ReturnLobby);
    }

    private void DisplayResult() {
        var isWinner = TurnManager.Instance.IsWinner();
        var win = "WIN";
        if (!isWinner) win = "LOOSE";
        resultDisplay.text = $"YOU {win}";
    }

    private static void ReturnHome() {
        NetworkManager.singleton.StopHost();
        //SceneManager.LoadScene("Garage", LoadSceneMode.Single);
    }

    private static void ReturnLobby() {
        NetworkManager.singleton.StopHost();
        //SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }
}
