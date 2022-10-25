using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableTile : MonoBehaviour {
	//[SerializeField] private GameObject selectedVisual;
	//[SerializeField] private Material validVisual;
	//[SerializeField] private Material invalidVisual;
	public Image tileSprite;
	//[SerializeField] private Material targetableVisual;

	public int tileX;
	public int tileY;
	public EncounterManager EMan;

	//void OnMouseUp() {
	//	if (!EventSystem.current.IsPointerOverGameObject()) {
	//		print($"Tile:{tileX},{tileY}");
	//		//EMan.CheckTileClick(tileX, tileY, false);
	//	}
	//}

	//void OnMouseOver() {
	//	if (Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject()) {
	//		//EMan.CheckTileClick(tileX, tileY, true);
	//	}
	//}
	public void HighlightTile(Color color) {
		tileSprite.color = color;
    }

	public void DeHighlightTile() {
		tileSprite.color = Color.white;
    }


	public void OnClick() {
		print($"Tile:{tileX},{tileY}");
		EMan.CheckClick(tileX, tileY);
	}
}
