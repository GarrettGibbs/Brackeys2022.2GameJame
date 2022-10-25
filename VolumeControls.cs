using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControls : MonoBehaviour {
    string _masterVolumeParameter = "MasterVolume";
    string _musicVolumeParameter = "MusicVolume";
    string _sfxVolumeParameter = "SFXVolume";
    string _sfxOnValue = "SFXOnValue";
    [SerializeField] AudioMixer _mainMixer;
    [SerializeField] Slider _mainSlider;
    [SerializeField] Toggle _mainToggle;
    [SerializeField] Slider _musicSlider;
    [SerializeField] Toggle _musicToggle;
    [SerializeField] Slider _sfxSlider;
    [SerializeField] Toggle _sfxToggle;
    [SerializeField] float _multiplier = 30f;
    [SerializeField] float _defaultVolume = .8f;
    private bool _disableToggleEvent;

    private void Awake() {
        _mainSlider.onValueChanged.AddListener(HandleMainSliderValueChanged);
        _mainToggle.onValueChanged.AddListener(HandleMainToggleChanged);
        _musicSlider.onValueChanged.AddListener(HandleMusicSliderValueChanged);
        _musicToggle.onValueChanged.AddListener(HandleMusicToggleChanged);
        _sfxSlider.onValueChanged.AddListener(HandleSfxSliderValueChanged);
        _sfxToggle.onValueChanged.AddListener(HandleSFXToggleChanged);
    }

    void Start() {
        _mainSlider.value = PlayerPrefs.GetFloat(_masterVolumeParameter, _mainSlider.value); //Loads previous slider settings from PlayerPrefs.
        _musicSlider.value = PlayerPrefs.GetFloat(_musicVolumeParameter, _musicSlider.value);
        _sfxSlider.value = PlayerPrefs.GetFloat(_sfxVolumeParameter, _sfxSlider.value);

        HandleMainSliderValueChanged(_mainSlider.value);
        HandleMusicSliderValueChanged(_musicSlider.value);
        HandleSfxSliderValueChanged(_sfxSlider.value);
    }

    private void HandleMainSliderValueChanged(float value) {
        if (value == 0) value = .0001f;
        _mainMixer.SetFloat(_masterVolumeParameter, Mathf.Log10(value) * _multiplier); // Adjusts settings for the Master mixer.
        _disableToggleEvent = true;
        _mainToggle.isOn = _mainSlider.value > _mainSlider.minValue;
        _disableToggleEvent = false;
    }

    private void HandleMusicSliderValueChanged(float value) {
        if (value == 0) value = .0001f;
        _mainMixer.SetFloat(_musicVolumeParameter, Mathf.Log10(value) * _multiplier); // Adjusts settings for the Music mixer.
        _disableToggleEvent = true;
        _musicToggle.isOn = _musicSlider.value > _musicSlider.minValue;
        _disableToggleEvent = false;
    }

    private void HandleSfxSliderValueChanged(float value) {
        if (value == 0) value = .0001f;
        _mainMixer.SetFloat(_sfxVolumeParameter, Mathf.Log10(value) * _multiplier); // Adjusts settings for the SFX mixer
        PlayerPrefs.SetFloat(_sfxOnValue, value); // Lets GameManager know if sfx are on
        _disableToggleEvent = true;
        _sfxToggle.isOn = _sfxSlider.value > _sfxSlider.minValue;
        _disableToggleEvent = false;
    }

    private void HandleMainToggleChanged(bool enableSound) {
        if (_disableToggleEvent) return;
        if (enableSound) {
            _mainSlider.value = .8f;
        } else {
            _mainSlider.value = _mainSlider.minValue;
        }
    }

    private void HandleMusicToggleChanged(bool enableSound) {
        if (_disableToggleEvent) return;
        if (enableSound) {
            _musicSlider.value = .8f;
        } else {
            _musicSlider.value = _musicSlider.minValue;
        }
    }

    private void HandleSFXToggleChanged(bool enableSound) {
        if (_disableToggleEvent) return;
        if (enableSound) {
            _sfxSlider.value = .8f;
        } else {
            _sfxSlider.value = _sfxSlider.minValue;
        }
    }

    private void OnDisable() {
        PlayerPrefs.SetFloat(_masterVolumeParameter, _mainSlider.value); //Saves current slider values to PlayerPrefs.
        PlayerPrefs.SetFloat(_musicVolumeParameter, _musicSlider.value);
        PlayerPrefs.SetFloat(_sfxVolumeParameter, _sfxSlider.value);
    }
}
