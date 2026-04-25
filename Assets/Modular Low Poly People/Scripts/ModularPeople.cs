using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLPP
{
    public static class Helper
    {
        // Get what is being worn by category(Hair, legs etc)
        public static string GetCurrentClothing(GameObject character, string category)
        {
            if (character.name != "ModularParts")
            {
                character = character.transform.Find("ModularParts").gameObject;
            }

            Transform categoryTR = character.transform.Find(category);

            foreach (Transform child in categoryTR)
            {
                if (child.gameObject.activeSelf)
                {
                    return child.name;
                }
            }

            return "None";
        }

        // Set next clothing by category
        public static void NextClothing(GameObject character, string category)
        {
            if (character.name != "ModularParts")
            {
                character = character.transform.Find("ModularParts").gameObject;
            }

            Transform categoryTR = character.transform.Find(category);

            bool noneActive = true;
            foreach (Transform child in categoryTR)
            {
                if (child.gameObject.activeSelf)
                {
                    noneActive = false;
                    child.gameObject.SetActive(false);

                    int currentIndex = child.GetSiblingIndex();

                    if (categoryTR.childCount - 1 == currentIndex)
                    {
                        // If category can be none
                        if (category != "Hair" && category != "FacialHair" && category != "Headwear" && category != "Facewear" && category != "Belt")
                        {
                            ChangeClothing(character, category, categoryTR.GetChild(0).name);
                        }
                    }
                    else
                    {
                        ChangeClothing(character, category, categoryTR.GetChild(currentIndex + 1).name);
                    }

                    break;
                }
            }

            if (noneActive)
            {
                if (categoryTR.childCount > 0)
                {
                    ChangeClothing(character, category, categoryTR.GetChild(0).name);
                }
            }
        }

        // Set previous clothing by category
        public static void PreviousClothing(GameObject character, string category)
        {
            if (character.name != "ModularParts")
            {
                character = character.transform.Find("ModularParts").gameObject;
            }

            Transform categoryTR = character.transform.Find(category);

            bool noneActive = true;
            foreach (Transform child in categoryTR)
            {
                if (child.gameObject.activeSelf)
                {
                    noneActive = false;
                    child.gameObject.SetActive(false);

                    int currentIndex = child.GetSiblingIndex();

                    if (0 == currentIndex)
                    {
                        // If category can be none
                        if (category != "Hair" && category != "FacialHair" && category != "Headwear" && category != "Facewear" && category != "Belt")
                        {
                            ChangeClothing(character, category, categoryTR.GetChild(categoryTR.childCount - 1).name);
                        }
                    }
                    else
                    {
                        ChangeClothing(character, category, categoryTR.GetChild(currentIndex - 1).name);
                    }

                    break;
                }
            }

            if (noneActive)
            {
                if (categoryTR.childCount > 0)
                {
                    ChangeClothing(character, category, categoryTR.GetChild(categoryTR.childCount - 1).name);
                }
            }
        }

        public static void Randomize(GameObject character, string category = "")
        {
            if (character.name != "ModularParts")
            {
                character = character.transform.Find("ModularParts").gameObject;
            }

            if (category == "")
            {
                int categoryCount = character.transform.childCount;

                bool hair = true; // 1/3 chance headwear will be applied
                if (UnityEngine.Random.Range(1, 4) == 3)
                {
                    hair = false;
                }

                foreach (Transform child in character.transform)
                {
                    if (!(child.name == "Hair" && hair == false) && !(child.name == "Headwear" && hair == true))
                    {
                        int categoryChildCount = child.childCount;

                        if (categoryChildCount != 0)
                        {
                            string randomClothName = child.GetChild(UnityEngine.Random.Range(0, categoryChildCount)).name;
                            ChangeClothing(character, child.name, randomClothName);
                        }
                    }
                }
            }
            else
            {
                Transform categoryTR = character.transform.Find(category);

                int categoryChildCount = categoryTR.childCount;

                string randomClothName = categoryTR.GetChild(UnityEngine.Random.Range(0, categoryChildCount)).name;
                ChangeClothing(character, categoryTR.name, randomClothName);
            }
        }

        // Change the clothing by name and category
        public static void ChangeClothing(GameObject character, string category, string name)
        {
            if (character.name != "ModularParts")
            {
                character = character.transform.Find("ModularParts").gameObject;
            }

            Transform categoryTR = character.transform.Find(category);

            Transform clothingTR = categoryTR.transform.Find(name);

            if (categoryTR == null)
            {
                return;
            }

            bool dontUnhide = false; // If set to none or can not be set because of clipping (belt)

            // Hair - hide headwear
            if (category == "Hair")
            {
                Transform headwearTR = character.transform.Find("Headwear");

                foreach (Transform child in headwearTR)
                {
                    child.gameObject.SetActive(false);
                }

                if (name == "none" || name == "None")
                {
                    dontUnhide = true;
                }
            }

            // Headwear - hide hair
            else if (category == "Headwear")
            {
                Transform hairTR = character.transform.Find("Hair");

                foreach (Transform child in hairTR)
                {
                    child.gameObject.SetActive(false);
                }

                if (name == "none" || name == "None")
                {
                    dontUnhide = true;
                }
            }

            // Facial hair
            else if (category == "FacialHair")
            {
                if (name == "none" || name == "None")
                {
                    dontUnhide = true;
                }
            }

            // Facewear
            else if (category == "Facewear")
            {
                if (name == "none" || name == "None")
                {
                    dontUnhide = true;
                }
            }

            // Topwear - hide or unhide legs` belt part and belt clothing
            else if (category == "Topwear")
            {
                // Legs` belt part
                Transform legsTR = character.transform.Find("Legs");

                foreach (Transform child in legsTR)
                {
                    if (child.gameObject.activeSelf)
                    {
                        if (name.EndsWith("_Belt"))
                        {
                            child.Find(child.name + "_Belt").gameObject.SetActive(true);
                        }
                        else
                        {
                            child.Find(child.name + "_Belt").gameObject.SetActive(false);

                            // Belt clothing
                            Transform beltTR = character.transform.Find("Belt");
                            string currentBelt = GetCurrentClothing(character, "Belt");

                            if (currentBelt != "None")
                            {
                                beltTR.Find(GetCurrentClothing(character, "Belt")).gameObject.SetActive(false);
                            }

                        }

                        // If suit then belt clothing should be deactivated
                        if (name.EndsWith("_Suit_Belt"))
                        {
                            // Belt clothing
                            Transform beltTR = character.transform.Find("Belt");
                            string currentBelt = GetCurrentClothing(character, "Belt");

                            if (currentBelt != "None") { 
                                beltTR.Find(GetCurrentClothing(character, "Belt")).gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            // Belt - hide if topwear does not allow belt
            else if (category == "Belt")
            {
                string currentTopwear = GetCurrentClothing(character, "Topwear");

                if (currentTopwear != "None")
                {
                    if (!currentTopwear.EndsWith("_Belt") || currentTopwear.EndsWith("_Suit_Belt"))
                    {
                        dontUnhide = true;
                    }
                }

                if (name == "none" || name == "None")
                {
                    dontUnhide = true;
                }
            }

            // Legs - hide or unhide belt and shin part of the legwear
            else if (category == "Legs")
            {
                Transform topwearTR = character.transform.Find("Topwear");

                // Belt
                string currentTopwear = GetCurrentClothing(character, "Topwear");

                if (currentTopwear != "None")
                {
                    if (currentTopwear.EndsWith("_Belt"))
                    {
                        clothingTR.Find(clothingTR.name + "_Belt").gameObject.SetActive(true);
                    }
                    else
                    {
                        clothingTR.Find(clothingTR.name + "_Belt").gameObject.SetActive(false);
                    }
                }

                // Shin
                string currentFoot = GetCurrentClothing(character, "Feet");

                if (currentFoot != "None")
                {
                    if (currentFoot.EndsWith("_Shin"))
                    {
                        clothingTR.Find(clothingTR.name + "_Shin").gameObject.SetActive(false);
                    }
                    else
                    {
                        clothingTR.Find(clothingTR.name + "_Shin").gameObject.SetActive(true);
                    }
                }
            }

            // Foot - hide or unhide legs` shin part
            else if (category == "Feet")
            {
                Transform legsTR = character.transform.Find("Legs");

                foreach (Transform child in legsTR)
                {
                    if (child.gameObject.activeSelf)
                    {
                        if (name.EndsWith("_Shin"))
                        {
                            child.Find(child.name + "_Shin").gameObject.SetActive(false);
                        }
                        else
                        {
                            child.Find(child.name + "_Shin").gameObject.SetActive(true);
                        }
                    }
                }
            }

            // Disable other parts
            foreach (Transform child in categoryTR)
            {
                child.gameObject.SetActive(false);
            }

            // Enable given clothing
            if (dontUnhide == false)
            {
                clothingTR.gameObject.SetActive(true);
            }
            else
            {
                clothingTR.gameObject.SetActive(false);
            }
            // Test
        }
    }
}
