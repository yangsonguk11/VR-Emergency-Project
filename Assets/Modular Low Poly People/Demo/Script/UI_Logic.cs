using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MLPP
{
    public class UI_Logic : MonoBehaviour
    {
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject characterFem;

        [SerializeField] private Button hairPrev;
        [SerializeField] private Button hairNext;
        [SerializeField] private TextMeshProUGUI hairText;

        [SerializeField] private Button headwearNext;
        [SerializeField] private Button headwearPrev;
        [SerializeField] private TextMeshProUGUI headwearText;

        [SerializeField] private Button facialNext;
        [SerializeField] private Button facialPrev;
        [SerializeField] private TextMeshProUGUI facialText;

        [SerializeField] private Button facewearNext;
        [SerializeField] private Button facewearPrev;
        [SerializeField] private TextMeshProUGUI facewearText;

        [SerializeField] private Button topwearNext;
        [SerializeField] private Button topwearPrev;
        [SerializeField] private TextMeshProUGUI topwearText;

        [SerializeField] private Button beltNext;
        [SerializeField] private Button beltPrev;
        [SerializeField] private TextMeshProUGUI beltText;

        [SerializeField] private Button legsNext;
        [SerializeField] private Button legsPrev;
        [SerializeField] private TextMeshProUGUI legsText;

        [SerializeField] private Button footNext;
        [SerializeField] private Button footPrev;
        [SerializeField] private TextMeshProUGUI footText;

        [SerializeField] private Button handNext;
        [SerializeField] private Button handPrev;
        [SerializeField] private TextMeshProUGUI handText;

        [SerializeField] private Button genderNext;
        [SerializeField] private Button genderPrev;
        [SerializeField] private TextMeshProUGUI genderText;

        [SerializeField] private Button animNext;
        [SerializeField] private Button animPrev;
        [SerializeField] private TextMeshProUGUI animText;

        [SerializeField] private Button randomize;

        // Start is called before the first frame update
        void Start()
        {
            TextUpdate();

            hairNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Hair"); TextUpdate(); });
            hairPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Hair"); TextUpdate(); });

            headwearNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Headwear"); TextUpdate(); });
            headwearPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Headwear"); TextUpdate(); });

            facialNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "FacialHair"); TextUpdate(); });
            facialPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "FacialHair"); TextUpdate(); });

            facewearNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Facewear"); TextUpdate(); });
            facewearPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Facewear"); TextUpdate(); });

            topwearNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Topwear"); TextUpdate(); });
            topwearPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Topwear"); TextUpdate(); });

            legsNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Legs"); TextUpdate(); });
            legsPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Legs"); TextUpdate(); });

            footNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Feet"); TextUpdate(); });
            footPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Feet"); TextUpdate(); });

            beltNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Belt"); TextUpdate(); });
            beltPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Belt"); TextUpdate(); });

            handNext.onClick.AddListener(delegate { MLPP.Helper.NextClothing(CurrentCharacter(), "Hands"); TextUpdate(); });
            handPrev.onClick.AddListener(delegate { MLPP.Helper.PreviousClothing(CurrentCharacter(), "Hands"); TextUpdate(); });

            genderNext.onClick.AddListener(GenderChange);
            genderPrev.onClick.AddListener(GenderChange);

            animNext.onClick.AddListener(AnimationChange);
            animPrev.onClick.AddListener(AnimationChange);

            randomize.onClick.AddListener(Randomize);
        }

        void Randomize()
        {
            if (UnityEngine.Random.Range(1, 3) == 2)
            {
                GenderChange();
            }
            MLPP.Helper.Randomize(CurrentCharacter());
        }

        void TextUpdate()
        {
            hairText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Hair");
            headwearText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Headwear");
            facialText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "FacialHair");
            facewearText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Facewear");
            topwearText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Topwear");
            legsText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Legs");
            footText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Feet");
            beltText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Belt");
            handText.text = MLPP.Helper.GetCurrentClothing(CurrentCharacter(), "Hands");
        }

        void GenderChange()
        {
            if (character.activeSelf)
            {
                character.SetActive(false);
                characterFem.SetActive(true);
                genderText.text = "Female";

            }
            else
            {
                character.SetActive(true);
                characterFem.SetActive(false);
                genderText.text = "Male";

            }
            TextUpdate();
            AnimationSet();
        }

        GameObject CurrentCharacter()
        {
            if (character.activeSelf)
            {
                return character;
            }
            else
            {
                return characterFem;
            }
        }

        void AnimationChange()
        {
            if (animText.text == "None")
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("None", false);
                CurrentCharacter().GetComponent<Animator>().SetBool("Idle", true);
                animText.text = "Idle";
            }
            else if (animText.text == "Idle")
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("Idle", false);
                CurrentCharacter().GetComponent<Animator>().SetBool("Walk", true);
                animText.text = "Walk";
            }
            else if (animText.text == "Walk")
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("Walk", false);
                CurrentCharacter().GetComponent<Animator>().SetBool("Dance", true);
                animText.text = "Dance";
            }
            else
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("Dance", false);
                CurrentCharacter().GetComponent<Animator>().SetBool("None", true);
                animText.text = "None";
            }
        }

        void AnimationSet()
        {
            CurrentCharacter().GetComponent<Animator>().SetBool("None", false);
            CurrentCharacter().GetComponent<Animator>().SetBool("Idle", false);
            CurrentCharacter().GetComponent<Animator>().SetBool("Walk", false);
            CurrentCharacter().GetComponent<Animator>().SetBool("Dance", false);

            if (animText.text == "None")
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("None", true);
            }
            else if (animText.text == "Idle")
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("Idle", true);
            }
            else if (animText.text == "Walk")
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("Walk", true);
            }
            else
            {
                CurrentCharacter().GetComponent<Animator>().SetBool("Dance", true);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
