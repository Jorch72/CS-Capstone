﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class EnvironmentWeatherSystem
{
    public string name = "System";

    [Header("Environmental")]
    public Gradient sunlight;
    public Gradient fog;
    public bool hasClouds = false;
    public Gradient cloudsHigh;
    public Gradient cloudsLow;
    [Range(0.0000001f, 1.0f)]
    public float cloudsDensityDay = 0.01f;
    [Range(0.0000001f, 1.0f)]
    public float cloudsDensityNight = 0.1f;
    public float cloudsStratification = 1.0f;
    public Vector3 windDirection = new Vector3(1.0f, 0.0f, 0.0f);
    [Range(0.0000001f, 1.0f)]
    public float fogDensityDay = 0.0005f;
    [Range(0.0000001f, 1.0f)]
    public float fogDensityNight = 0.001f;
    [Header("Euler Rotations")]
    public Vector3 day;
    public Vector3 night;
    [Header("Transition")]
    [Range(0.0f, 1.0f)]
    public float transitionStart = 0.49f;
    [Range(0.0f, 1.0f)]
    public float transitionEnd = 0.51f;

    [Header("City")]
    public Gradient cityDiffuse;
    public AnimationCurve cityMetallic;
    public AnimationCurve citySmoothness;
    public AnimationCurve cityLightRange;

    [Header("Water")]
    public Gradient waterDiffuse;
    public AnimationCurve waterSmoothness;

    [Header("System Duration")]
    public int durationMin = 1;
    public int durationMax = 10;

    private EnvironmentController controller;

    public void Start(EnvironmentController controller)
    {
        this.controller = controller;
    }

    public void Update()
    {
        float evaluate = controller.dayCurrent;

        controller.light.color = sunlight.Evaluate(evaluate);
        RenderSettings.fogColor = fog.Evaluate(evaluate);

        if (evaluate >= transitionStart && evaluate <= transitionEnd)
        {
            EvaluateTimeOfDay(evaluate);
        }
        else if (evaluate < 0.5)
        {
            // NIGHT
            if (controller.isDay == true)
            {
                EvaluateTimeOfDay(0);

                controller.isDay = false;
            }
        }
        else
        {
            // DAY
            if (controller.isDay == false)
            {
                EvaluateTimeOfDay(1);

                controller.isDay = true;
            }
        }

        if (hasClouds)
            EvaluateWind(evaluate);
    }

    public void EvaluateWind(float evaluate)
    {
        float cloudsDensity = Mathf.Lerp(cloudsDensityNight, cloudsDensityDay, evaluate);

        for (int i = 0; i < controller.clouds.Length; i++)
        {
            float strata = (float)(i * cloudsStratification) / ((controller.clouds.Length - 1) / cloudsDensity);
            controller.clouds[i].SetTextureOffset("_MainTex", windDirection * Time.time);
        }
    }

    private void EvaluateTimeOfDay(float evaluate)
    {
        float range = (evaluate - transitionStart) / (transitionEnd - transitionStart);

        controller.light.transform.rotation = Quaternion.Euler(Vector3.Lerp(night, day, range));
        RenderSettings.fogDensity = Mathf.Lerp(fogDensityNight, fogDensityDay, range);
      
        if (hasClouds)
        {
            Color cloudHigh = cloudsHigh.Evaluate(range);
            Color cloudLow = cloudsLow.Evaluate(range);

            float cloudsDensity = Mathf.Lerp(cloudsDensityNight, cloudsDensityDay, range);

            for (int i = 0; i < controller.clouds.Length; i++)
            {
                float strata = (float)(i * cloudsStratification) / ((controller.clouds.Length - 1) / cloudsDensity);
                Color colour = Color.Lerp(cloudLow, cloudHigh, strata);
                colour.a = strata;
                controller.clouds[i].SetColor("_Color", colour);
                controller.clouds[i].SetColor("_Sun", controller.light.color);
            }
        }
        else
        {
            for (int i = 0; i < controller.clouds.Length; i++)
            {
                controller.clouds[i].SetColor("_TintColor", new Color(0,0,0,0));
            }
        }
    }
}
