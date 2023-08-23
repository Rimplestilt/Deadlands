﻿using UnityEngine;

namespace Deadlands.Nomad;

sealed class Wings
{
    private readonly PlayerGraphics _pGraphics;

    private readonly int _startSprite;

    private readonly float _size = 0.12f;
    private readonly float _pointiness = 0.7f; // Ranges from 0 - 1, one being max, zero being none at all



    public Wings(PlayerGraphics owner, int startSprite, float size = 0.12f, float pointiness = 0.7f)
    {
        this._pGraphics = owner;
        this._startSprite = startSprite;

        this._size = size;
        this._pointiness = pointiness;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Debug.Log(sLeaser.sprites.Length);
            
        for (int i = 0; i < 2; i++)
        {
            // All indices are realized clock-wise (Probably doesn't matter cause it's a 2D game, but I did it anyways)
            TriangleMesh.Triangle[] tris = {
                new(0, 2, 1),
                new(1, 2, 3),
                new(2, 4, 3),
                new(3, 4, 5)
            };
            
            Debug.Log(_startSprite + i);
            sLeaser.sprites[_startSprite + i] = new TriangleMesh("Futile_White", tris, false);
        }
    }


    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        for (int i = 0; i < 2; i++)
        {
            PlaceWingVerts(sLeaser, timeStacker, camPos, _startSprite + i, i);
        }
    }

    private void PlaceWingVerts(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos, int startSprite, int index)
    {
        /////////////////////////////////////
        // Base vertices
        /////////////////////////////////////

        Vector2 shoulder = Vector2.Lerp(_pGraphics.owner.bodyChunks[0].lastPos, _pGraphics.owner.bodyChunks[0].pos, timeStacker);
        Vector2 hip = Vector2.Lerp(_pGraphics.owner.bodyChunks[1].lastPos, _pGraphics.owner.bodyChunks[1].pos, timeStacker);

        // All Vector2s initalized with the type "var" in this context are not vertices
        var shoulderHipDiff = (shoulder - hip).normalized;

        Vector2 hand = Vector2.Lerp(_pGraphics.hands[0 + index].lastPos, _pGraphics.hands[0 + index].pos, timeStacker)
            + 2 * shoulderHipDiff;


        /////////////////////////////////////
        // Inbetween vertices
        /////////////////////////////////////

        
        var handShoulderDist = 0.1f * Vector2.Distance(hand, shoulder);
        handShoulderDist = Mathf.Min(handShoulderDist, 24); // Should never go above 24 (happens when throwing)
        

        Vector2 handShoulderInbetween = Vector2.Lerp(shoulder, hand, 0.5f); // Halfway between hand and shoulder


        shoulder += 3 * Vector2.up; // Move up by 3 ...pixels?


        var handHipsDiff = index == 0 ?
            hip - hand :
            hand - hip; // Should be swapped for left and right wings

        float handHipsDiffMagnitude = Mathf.Min(handHipsDiff.magnitude, 30); // Should never go above 30 (happens when throwing)
        handHipsDiff = handHipsDiff.normalized;

        Debug.Log(handShoulderDist * handHipsDiffMagnitude);

        var handHipsInbetween =
            Vector2.Lerp(hip, hand, 0.5f) + // Halfway between hips and hand
            
            _size * (handShoulderDist * handHipsDiffMagnitude) * // Calculate magnitude
            new Vector2(handHipsDiff.y, -handHipsDiff.x); // rotate 90 degrees

        Vector2 handToInbetween = Vector2.Lerp(hand, handHipsInbetween, _pointiness); // Inbetween (From hand_shoulder_inbetween -- to hand
        Vector2 hipsToInbetween = Vector2.Lerp(hip, handHipsInbetween, _pointiness); // Inbetween (From hand_shoulder_inbetween -- to hips

        /////////////////////////////////////
        // Vertex assignment
        /////////////////////////////////////

        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(0, hand - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(1, handToInbetween - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(2, handShoulderInbetween - camPos);

        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(3, hipsToInbetween - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(4, shoulder - camPos);
        ((TriangleMesh)sLeaser.sprites[startSprite]).MoveVertice(5, hip - camPos);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[_startSprite + i].color = sLeaser.sprites[0].color; //PlayerGraphics.SlugcatColor((pGraphics.owner as Player).SlugCatClass); //(pGraphics.owner as Player).ShortCutColor();
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        for (int i = 0; i < 2; i++)
        {
            newContainer ??= rCam.ReturnFContainer("Midground");
            newContainer.AddChild(sLeaser.sprites[_startSprite + i]);

            sLeaser.sprites[_startSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
        }
    }
}
