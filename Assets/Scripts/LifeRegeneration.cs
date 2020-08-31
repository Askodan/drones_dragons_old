using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Life))]

public class LifeRegeneration : MonoBehaviour {
    public DragonParameters DP;
    public VehicleParameters VP;
    private Life life;
    private DragonBrain DB;
    // Use this for initialization
    void Start () {
        life = GetComponent<Life>();
        DB = GetComponent<DragonBrain>();
    }
	
	// Update is called once per frame
	void Update () {
        //analiza stanu zycia
        float regenerationBonus = 0f;
        float refeulingBonus = 0f;

        if (DB)
        {
            DP.Update(ref regenerationBonus, ref refeulingBonus, DB);
        }
        else
        {
            VP.Update();
        }
        life.recovery = regenerationBonus;
        life.refuely = refeulingBonus;
    }

    [System.Serializable]
    public class DragonParameters
    {
        public float regenerationBonus_standing = 3f;
        public float regenerationBonus_crawling = 1f;
        public float regenerationBonus_running = 0.5f;
        public float regenerationBonus_flying = 0.5f;

        private float refuelingBonus_standing = 5f;
        private float refuelingBonus_crawling = 2f;
        private float refuelingBonus_running = 1f;
        private float fuelUsage_flying = 1f;
        private float fuelUsage_firing = 5f;

        public void Update(ref float regenerationBonus, ref float refuelingBonus, DragonBrain DB)
        {
            if (!DB.DM.flying)
            {
                switch (DB.DM.ground3.GroundedState)
                {
                    case 0:
                        regenerationBonus += regenerationBonus_standing;
                        refuelingBonus += refuelingBonus_standing;
                        break;
                    case 1:
                        regenerationBonus += regenerationBonus_crawling;
                        refuelingBonus += refuelingBonus_crawling;
                        break;
                    case 2:
                        regenerationBonus += regenerationBonus_running;
                        refuelingBonus += refuelingBonus_running;
                        break;
                }
            }
            else
            {
                regenerationBonus += regenerationBonus_flying;
                refuelingBonus -= fuelUsage_flying;
            }

            if (DB.firing)
            {
                refuelingBonus -= fuelUsage_firing;
            }
        }
    }

    [System.Serializable]
    public class VehicleParameters
    {
        public float regenerationBonus_Repairing = 10f;
        private float refuelingBonus_Refueling = 10f;
        private float fuelConsuptionRate = 1f;

        public void Update()
        {

        }
    }
}
