using UnityEngine;
using UnityEngine.UI;


public class ChargeUI : MonoBehaviour
{
public PlayerController player;
public Image chargeFill; // radial fill
public Text sizeText;


void Update()
{
if (player == null) return;
// if charging, show fill based on preview projectile size
// For simplicity attempt to read current preview via reflection-safe way
// (In production expose preview progress via PlayerController property)
}
}