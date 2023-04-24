using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Esto es simplemente una clase en la que almacenamos constantes como el número de cartas o cúando se realiza blackjack
internal static class Constants
{
    public const int DeckCards = 52;
    public const int Blackjack = 21;
    public const int DealerStand = 17;
    public const int SoftAce = 11;
    public const int ProbPrecision = 2;
}
public class Deck : MonoBehaviour
{   
    //Variables globales
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;
    public Text pointsMessage;
    public Text dealerPointsMessage;
    public Text bank;
    public int bankValue;
    public int apuesta;
    public Text apuestaText;
    public Dropdown desplegable;

    public int[] values = new int[Constants.DeckCards];
    int cardIndex = 0;    
    //-------------------------------------------------------------------------------------------
    private void Awake()
    {    
        InitCardValues();        
        List<string> opcionesDeCredito = new List<string>() {"10","100","1000"};
        this.desplegable.AddOptions(opcionesDeCredito);
    }
    //-------------------------------------------------------------------------------------------
    public void SeleccionarCuantosCreditos()
    {
        string creditosSeleccionados = this.desplegable.captionText.text;

        apuesta = int.Parse(creditosSeleccionados);
        bankValue -= apuesta;
        bank.text = "Tienes " + bankValue + " €";

        this.desplegable.interactable = false;
        hitButton.interactable = true;
        stickButton.interactable = true;

        apuestaText.text = "";
    }
    //-------------------------------------------------------------------------------------------
    private void Start()
    {
        ShuffleCards();
        bank.text = "Tienes " + bankValue + " €";
        StartGame();        
    }
    //-------------------------------------------------------------------------------------------
    private void InitCardValues()
    {
        /*TODO:
         * Asignar un valor a cada una de las 52 cartas del atributo "values".
         * En principio, la posición de cada valor se deberá corresponder con la posición de faces. 
         * Por ejemplo, si en faces[1] hay un 2 de corazones, en values[1] debería haber un 2.
         */

        /*Se que, a lo mejor, con switch todo se vería más bonito en cuanto a código
        a la par que racional, pero mi mente se aclara más con el uso de 'if','else if'
        y 'else' que usando un 'switch' y muchos 'case' y 'break' a la hora de
        pensar algoritmos de programación.
        */

        // Resumen del for:
        /* Del primer 'if' (linea 48) hasta el 'else' (linea 80) asignamos
        valores (11, i+1, 10, i-2, 10, i-25, 10, i-38) si la posición 'i' es
        menor que (10, 13, 23, 26, 36, 39, 48) respectivamente.
        Sino, el valor será 10*/
        for (int i = 0; i < values.Length; i++)
        {
            if (i == 0 || i == 13 || i == 26 || i == 39)
            {
                values[i] = 11;
            }
            else if (i < 10)
            {
                values[i] = i + 1;
            }
            else if (i < 13)
            {
                values[i] = 10;
            }
            else if (i < 23)
            {
                values[i] = i - 12;
            }
            else if (i < 26)
            {
                values[i] = 10;
            }
            else if (i < 36)
            {
                values[i] = i - 25;
            }
            else if (i < 39)
            {
                values[i] = 10;
            }
            else if (i < 48)
            {
                values[i] = i - 38;
            }
            else
            {
                values[i] = 10;
            }
        }
    }
    //-------------------------------------------------------------------------------------------
    private void ShuffleCards()
    {
        /*TODO:
         * Barajar las cartas aleatoriamente.
         * El método Random.Range(0,n), devuelve un valor entre 0 y n-1
         * Si lo necesitas, puedes definir nuevos arrays.
         */
         for (int i = 0; i < values.Length; i++)
        {
            int j = Random.Range(0, 52); //Valor Random entre 0 y 51 (52-1)
            int temp_value = values[i]; 
            var temp_faces = faces[i]; //Pongo var porque faces[i] es del tipo 'UnityEngine.Sprite'.
                                       //A su vez, me resulta más intuitivo al explicarlo en la exposición

            //Face Random
            faces[i] = faces[j];
            faces[j] = temp_faces;

            //Valor Random
            values[i] = values[j];
            values[j] = temp_value;
        }
    }
    //-------------------------------------------------------------------------------------------
    void StartGame()
    {
        if (bankValue >= 10)
        {
            for (int i = 0; i < 2; i++)
            {
                PushPlayer();
                PushDealer();
                hitButton.interactable = false;
                stickButton.interactable = false;
                /*TODO:
                 * Si alguno de los dos obtiene Blackjack, termina el juego y mostramos mensaje
                 */

                pointsMessage.text = "Tienes " + player.GetComponent<CardHand>().points + " puntos";
            }
        }
        else
        {
            finalMessage.text = "Te has quedado sin un duro. Pulsa 'Nuevo juego' para rellenar la banca";
            this.desplegable.interactable = false;
            hitButton.interactable = false;
            stickButton.interactable = false;
            bankValue = 1000;
        }
    }
    //-------------------------------------------------------------------------------------------
    private void CalculateProbabilities()
    {
        float possibleCases = values.Length - cardIndex + 1.0f;

        // Every remaining ace has two possible values (different sums)
        for (int i = cardIndex; i < values.Length; ++i)
        {
            if (values[i] == 1) { possibleCases++; }
        }
        /*TODO:
         * Calcular las probabilidades de:
         * - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
         * - Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
         * - Probabilidad de que el jugador obtenga más de 21 si pide una carta          
         */
        probMessage.text = "Deal > Player:         "+ProbabilidadDealerMayorQuePlayer(possibleCases) + " % "+ System.Environment.NewLine + 
            "17 <= Carta <= 21: "+ProbabilidadPlayerEntre17y21(possibleCases) + " % "+ System.Environment.NewLine + 
            "Carta > 21:             "+ProbabilidadPlayerMayorQue21() + " %";
    }
    //-------------------------------------------------------------------------------------------
    // Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
    private double ProbabilidadDealerMayorQuePlayer(float possibleCases)
    {
        CardHand dealerHand = dealer.GetComponent<CardHand>();
        //List<CardModel> dealercards;
        List<CardModel> dealerCards = dealerHand.cards
            .Select(card => card.GetComponent<CardModel>()).ToList();

        int favorableCases = 0;
        if (dealerCards.Count > 1) 
        {
            int dealerPointsVisible = dealerCards[1].value;

            int playerPoints = player.GetComponent<CardHand>().points;
            int sum = 0;

            for (int i = cardIndex; i < values.Length; ++i)
            {
                // Caso por Defecto
                sum = dealerPointsVisible + values[i];
                if (sum < Constants.Blackjack && sum > playerPoints)
                {
                    favorableCases++;
                }

                // Un As oculto como 11 puntos
                if (values[i] == 1)
                {
                    sum = dealerPointsVisible + Constants.SoftAce;
                    if (sum < Constants.Blackjack && sum > playerPoints)
                    {
                        favorableCases++;
                    }
                }

                // Un As visible como 11 puntos
                if (dealerPointsVisible == 1)
                {
                    sum = Constants.SoftAce + values[i]; // SoftAce == As suave
                    if (sum < Constants.Blackjack && sum > playerPoints)
                    {
                        favorableCases++;
                    }
                }
            }
        }
        /*Con System.Math.Round redondeamos el número a tantos decimales como tenga
        Constants.ProbPrecision (en este caso, a 2 decimales)*/
        return System.Math.Round((favorableCases / possibleCases) * 100, Constants.ProbPrecision);
    }
    //-------------------------------------------------------------------------------------------
    // Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
    private double ProbabilidadPlayerEntre17y21(float possibleCases)
    {
        int playerPoints = dealer.GetComponent<CardHand>().points;
        int favorableCases = 0;
        int sum = 0;

        for (int i = cardIndex; i < values.Length; ++i)
        {
            sum = playerPoints + values[i];
            if (sum >= Constants.DealerStand && sum <= Constants.Blackjack)
            {
                favorableCases++;
            }

            // Contemplate an ace as 11 points
            if (values[i] == 1)
            {
                sum = playerPoints + Constants.SoftAce;
                if (sum >= Constants.DealerStand && sum <= Constants.Blackjack)
                {
                    favorableCases++;
                }
            }
        }
    
        return System.Math.Round((favorableCases / possibleCases) * 100, Constants.ProbPrecision);
    }
    //-------------------------------------------------------------------------------------------
    // Probabilidad de que el jugador obtenga más de 21 si pide una carta 
    private double ProbabilidadPlayerMayorQue21()
    {
        float possibleCases = values.Length - cardIndex + 1.0f;
        int playerPoints = player.GetComponent<CardHand>().points;
        int favorableCases = 0;
        int sum = 0;

        for (int i = cardIndex; i < values.Length; ++i)
        {
            sum = playerPoints + values[i];
            if (sum > Constants.Blackjack) { favorableCases++; }
        }

        return System.Math.Round((favorableCases / possibleCases) * 100, Constants.ProbPrecision);
    }
    //-------------------------------------------------------------------------------------------
    void PushDealer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;        
    }
    //-------------------------------------------------------------------------------------------
    void PushPlayer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        CalculateProbabilities();
    }       
    //-------------------------------------------------------------------------------------------
    public void Hit()
    {
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */
        
        //Repartimos carta al jugador
        PushPlayer();
        pointsMessage.text = "Tienes " + player.GetComponent<CardHand>().points + " puntos";
        /*TODO:
         * Comprobamos si el jugador ya ha perdido y mostramos mensaje
         */
        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "La banca gana";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }
    //-------------------------------------------------------------------------------------------
    public void Stand()
    {
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */

        /*TODO:
         * Repartimos cartas al dealer si tiene 16 puntos o menos
         * El dealer se planta al obtener 17 puntos o más
         * Mostramos el mensaje del que ha ganado
         */  
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        hitButton.interactable = false;
        stickButton.interactable = false;   

        while (dealer.GetComponent<CardHand>().points < 17)
        {
            PushDealer();
        }
        int dealerPoints = dealer.GetComponent<CardHand>().points;
        int playerPoints = player.GetComponent<CardHand>().points;
        dealerPointsMessage.text = "El dealer ha conseguido "+ dealerPoints + " puntos";

        if (dealerPoints == 21)
        {
            finalMessage.text = "El dealer hizo BlackJack. Has perdido";
        }
        else if (playerPoints == 21)
        {
            finalMessage.text = "Enhorabuena, has hecho BlackJack. Has ganado " + (apuesta * 2).ToString() + " €";
            bankValue += apuesta * 2;
            bank.text = "Tienes " + bankValue + " €";
        }
        else if (dealerPoints > 21)
        {
            finalMessage.text = "La banca se ha pasado, has ganado " + (apuesta * 2).ToString()+" €";
            bankValue += apuesta * 2;
            bank.text = "Tienes " + bankValue + " €";
        }
        else if (dealerPoints == playerPoints)
        {
            //Si hay empate, simplemente se devuelve lo que se ha apostado
            finalMessage.text = "Habeis tenido un empate";
            bankValue += apuesta;
            bank.text = "Tienes " + bankValue + " €";
        }
        else if (dealerPoints > playerPoints)
        {
            finalMessage.text = "La banca te ha superado, has perdido";
        }
        else
        {
            finalMessage.text = "Has ganado "+(apuesta*2).ToString()+" € a la banca. Enhorabuena.";
            bankValue += apuesta * 2;
            bank.text = "Tienes " + bankValue + " €";
        }
    }
    //-------------------------------------------------------------------------------------------
    public void PlayAgain()
    {
        this.desplegable.interactable = true;
        finalMessage.text = "";
        dealerPointsMessage.text = "";
        apuestaText.text = "Elije una apuesta:";
        bank.text = "Tienes " + bankValue + " €";
//        hitButton.interactable = true;
  //      stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }
}