﻿using System;
using UnityEngine;

namespace BomberMan {
    public class Explosion : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D other) {
            print("Detected collision between " + gameObject.name + " and " + other.gameObject.name);
            
            if (other.gameObject.CompareTag("Player") && !other.GetComponent<Player>().IsDead()) other.gameObject.GetComponent<Player>().OnDeath();
            if (other.gameObject.CompareTag("Enemy") && !other.GetComponent<Enemy>().IsDead()) other.gameObject.GetComponent<Enemy>().OnDeath();
        }
    }
}