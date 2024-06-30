using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JigsawFeud.JigsawPuzzle;
using System.Reflection;
using JigsawFeud.TouchSystem;
using System;

namespace JigsawFeud
{
    public enum DrawerScrollState
    {
        AtRest,
        Scrolling,
        Breaking, //Not needed so far
        Dragged,
        BouncingBack,
        Disabled
    }

    public class DrawerScroll
    {
        //TODO: DrawerScroll should get disabled when scrolling is not required. ie. (diffTopBottomPiece < diffTopBottomEdge)
        //TODO: Bring all the logic related to dragging and scrolling inside this class

        //reference to the same list of Pieces that is there in Drawer class
        private string m_drawerName;
        private List<JigsawPiece> m_listOfPieces;

        private float m_drawerTop;
        private float m_drawerBottom;

        private DrawerScrollState m_state;
        private float m_scrollVelocity; /* -ve = downward; +ve = upward */
        private float m_deceleration; /* the opposing acceleration that (kind-of) works like a friction */
        private float m_bounceBackDistance; /* the distance remaining to bounce-back */
        // Start is called before the first frame update

        public DrawerScroll(string drawerName,
                            List<JigsawPiece> listOfPieces, 
                            float drawerTop, 
                            float drawerBottom)
        {
            m_drawerName = drawerName;
            m_listOfPieces = listOfPieces;
            m_drawerTop = drawerTop;
            m_drawerBottom = drawerBottom;
            m_state = DrawerScrollState.AtRest;
            m_scrollVelocity = 0;
            m_bounceBackDistance = 0;
            EnableScrolling();
        }

        public void DisableScrolling()
        {
            TouchSystemEvents.DrawerDragBeginEvent -= HandleDrawerDragBeginEvent;
            TouchSystemEvents.DrawerDragMoveEvent -= HandleDrawerDragMoveEvent;
            TouchSystemEvents.DrawerDragEndEvent -= HandleDrawerDragEndEvent;
            TouchSystemEvents.DrawerScrolledEvent -= HandleDrawerScrollEvent;
            StopScrollingImmediately();
            ChangeState(DrawerScrollState.Disabled);
            Debug.Log("Disabled Scrolling");
        }

        public void EnableScrolling()
        {
            /* TODO: Perhaps, it is required to remove all these event-handlers
             * when the scroller is disabled.
             * In such a case, we should be able to add the event-handlers
             * again when the scroller is enabled. 
             * Some actions should be done only when the scroller is enabled.
             * Think this through and make appropriate changes.
             */

            TouchSystemEvents.DrawerDragBeginEvent += HandleDrawerDragBeginEvent;
            TouchSystemEvents.DrawerDragMoveEvent += HandleDrawerDragMoveEvent;
            TouchSystemEvents.DrawerDragEndEvent += HandleDrawerDragEndEvent;
            TouchSystemEvents.DrawerScrolledEvent += HandleDrawerScrollEvent;
            Debug.Log("Disabled Scrolling");
        }

        public void Update()
        {
            float deltaScroll = CalculateScrollOffset(Time.deltaTime);
            ApplyScrollOffsetToAllPieces(deltaScroll);
        }

        public void LateUpdate()
        {
            //CheckScrollContentHeight(); //TODO: This could be handled elsewhere to avoid calling it in every frame
            CheckBounds();
        }

        public float CalculateScrollOffset(float deltaTime)
        {
            float scrollOffset = 0;
            switch (m_state)
            {
                case DrawerScrollState.Scrolling:
                    {
                        scrollOffset = m_scrollVelocity * deltaTime;
                        ApplyFrictionWhileScrolling();
                    }
                    break;
                case DrawerScrollState.Breaking:
                    break;
                case DrawerScrollState.BouncingBack:
                    {
                        float speed = Mathf.Abs(m_scrollVelocity);
                        int direction = (speed == 0) ? 1 : (int)(m_scrollVelocity / speed);
                        scrollOffset = m_scrollVelocity * deltaTime;
                        float scalarScrollOffset = Mathf.Abs(scrollOffset);

                        if (m_bounceBackDistance <= scalarScrollOffset) 
                        {
                            /* Remaining distance to cover is less than calculated offset */
                            scrollOffset = scalarScrollOffset * direction;
                            StopScrollingImmediately();
                        }
                        else
                        {
                            m_bounceBackDistance -= scalarScrollOffset;                           
                        }
                    }
                    break;
                case DrawerScrollState.AtRest:
                    break;
                case DrawerScrollState.Dragged:
                    break;
                case DrawerScrollState.Disabled:
                    break;
            }
            return scrollOffset;
        }

        private void ApplyScrollOffsetToAllPieces(float scrollOffset)
        {
            foreach (JigsawPiece piece in m_listOfPieces)
            {
                float localPositionZ = piece.transform.localPosition.z;
                localPositionZ += scrollOffset;
                piece.transform.localPosition = new Vector3(piece.transform.localPosition.x,
                                                                    piece.transform.localPosition.y,
                                                                    localPositionZ);
            }
        }

        private void ApplyFrictionWhileScrolling()
        {
            /* Separating the speed and direction */
            float speed = Mathf.Abs(m_scrollVelocity);
            int direction = (speed == 0) ? 1 : (int)(m_scrollVelocity / speed);
            float dSpeed = m_deceleration * Time.deltaTime;
            /* Reducing Speed due to friction/deceleration */
            speed -= dSpeed;
            if (speed <= 0)
            {
                StopScrollingImmediately();
                return;
            }
            if (speed > Constants.MAX_SCROLL_SPEED)
            {
                speed = Constants.MAX_SCROLL_SPEED;
            }
            m_scrollVelocity = speed * direction;
        }

        private void CheckScrollContentHeight()
        {
            float topPieceTopEdge = getTopPieceTopEdge();
            float bottomPieceBottomEdge = getBottomPieceBottomEdge();
            float diffTopBottomEdge = Mathf.Abs(m_drawerBottom - m_drawerTop);
            float diffTopBottomPiece = Mathf.Abs(bottomPieceBottomEdge - topPieceTopEdge);
            if (diffTopBottomPiece <= diffTopBottomEdge)
            {
                //return;
                DisableScrolling();
                //TODO: Fix the call below 
                //ResetAllPiecesPositions();
            }
            else //TODO: remove this else, once disabling and enabling logics are implemented
            {
                //m_drawerScroll.StopScrollingImmediately();
                //ResetAllPiecesPositions();
            }
        }

        private float getTopPieceTopEdge()
        {
            JigsawPiece topPiece = m_listOfPieces[0];
            float topPieceTopEdge = topPiece.transform.localPosition.z - topPiece.GetVerticalHalfExtent();
            return topPieceTopEdge;
        }

        private float getBottomPieceBottomEdge()
        {
            JigsawPiece bottomPiece = m_listOfPieces[m_listOfPieces.Count - 1];
            float bottomPieceBottomEdge = bottomPiece.transform.localPosition.z + bottomPiece.GetVerticalHalfExtent();
            return bottomPieceBottomEdge;
        }

        private void CheckBounds()
        {
            if (m_listOfPieces.Count == 0)
            {
                return;
            }

            float topPieceTopEdge = getTopPieceTopEdge();
            float bottomPieceBottomEdge = getBottomPieceBottomEdge();
            float diffTopBottomEdge = Mathf.Abs(m_drawerBottom - m_drawerTop);
            float diffTopBottomPiece = Mathf.Abs(bottomPieceBottomEdge - topPieceTopEdge);
            if (diffTopBottomPiece <= diffTopBottomEdge) //TODO: Remove this when disabling logic is properly implemented
            {
                return;
            }

            switch (m_state)
            {
                case DrawerScrollState.Scrolling:
                    CheckTopBound(m_drawerTop, topPieceTopEdge);
                    CheckBottomBound(m_drawerTop, m_drawerBottom, topPieceTopEdge, bottomPieceBottomEdge);
                    break;
                case DrawerScrollState.AtRest:
                    break;
                case DrawerScrollState.Breaking:
                    break;
                case DrawerScrollState.Dragged:
                    CheckTopBound(m_drawerTop, topPieceTopEdge);
                    CheckBottomBound(m_drawerTop, m_drawerBottom, topPieceTopEdge, bottomPieceBottomEdge);
                    break;
                case DrawerScrollState.BouncingBack:
                    break;
            }
        }

        private void SelectDecelerationBasedOnScrollState()
        {
            switch (m_state)
            {
                case DrawerScrollState.Scrolling:
                    m_deceleration = Constants.SCROLL_DECELERATION;
                    break;
                case DrawerScrollState.BouncingBack:
                    m_deceleration = 0;
                    break;
                case DrawerScrollState.AtRest:
                    m_deceleration = 0;
                    break;
                case DrawerScrollState.Breaking:
                    m_deceleration = 0;
                    break;
                case DrawerScrollState.Dragged:
                    m_deceleration = 0;
                    break;
                case DrawerScrollState.Disabled:
                    m_deceleration = 0;
                    break;
            }
        }

        private void CheckTopBound(float drawerTopEdge, float topPieceTopEdge)
        {
            /* top of the 0th piece of the list shouldn't be below the top bound */
            if (topPieceTopEdge > drawerTopEdge)
            {
                float distance = topPieceTopEdge - drawerTopEdge;
                int direction = -1;
                BounceBack(direction, distance);
            }
        }

        private void CheckBottomBound(float drawerTopEdge, float drawerBottomEdge, float topPieceTopEdge, float bottomPieceBottomEdge)
        {
            /* bottom of the last piece of the list shouldn't be above the bottom bound 
               Unless the topEdge is above the topEdge of top-piece (not scrollable)
             */

            if (bottomPieceBottomEdge < drawerBottomEdge)
            {
                float distance = drawerBottomEdge - bottomPieceBottomEdge;
                int direction = 1;
                BounceBack(direction, distance);
            }
        }

        private void ChangeState(DrawerScrollState state)
        {
            m_state = state;
            SelectDecelerationBasedOnScrollState();
        }

        public void StartScrollingWithVelocity(float velocity)
        {
            m_scrollVelocity = velocity;
            ChangeState(DrawerScrollState.Scrolling);
        }

        public void BounceBack(int direction, float distance)
        {
            /* bounce back the scroll movement by the given distance in the given direction */
            m_bounceBackDistance = distance;
            float speed = Constants.SCROLL_BOUNCE_BACK_SPEED;
            m_scrollVelocity = speed * direction;
            ChangeState(DrawerScrollState.BouncingBack);            
        }

        public void StopScrollingImmediately()
        {
            m_scrollVelocity = 0;
            ChangeState(DrawerScrollState.AtRest);
        }

        private void HandleDrawerDragBeginEvent(String drawerName)
        {
            if (drawerName != m_drawerName)
            {
                return;
            }
            NotifyDragBegin();
        }

        private void HandleDrawerDragMoveEvent(String drawerName, float displacement)
        {
            if (drawerName != m_drawerName)
            {
                return;
            }

            if (IsBeingDragged)
            {
                CalculateAndApplyDragDisplacement(displacement);
            }
        }

        private void HandleDrawerDragEndEvent(String drawerName, float displacement)
        {
            if (drawerName != m_drawerName)
            {
                return;
            }

            if (IsBeingDragged)
            {
                CalculateAndApplyDragDisplacement(displacement);
                NotifyDragEnded();
            }

        }

        private void HandleDrawerScrollEvent(String drawerName, float velocity)
        {
            if (drawerName != m_drawerName) { return; }
            StartScrollingWithVelocity(-velocity);
        }

        private void CalculateAndApplyDragDisplacement(float displacement)
        {
            /* During LateUpdate, it shall be checked if drawer is dragged beyond top or bottom bounds, 
             * the drawerScroll shall internally change its state to bounce-back
             * and ultimately come to rest.
             */
            ApplyScrollOffsetToAllPieces(-displacement);
        }

        public void NotifyDragBegin()
        {
            ChangeState(DrawerScrollState.Dragged);
        }

        public void NotifyDragEnded()
        {
            ChangeState(DrawerScrollState.AtRest);
        }

        #region getters and setters

        public bool IsScrolling
        {
            get { return m_state == DrawerScrollState.Scrolling; }
        }

        public bool IsBeingDragged
        {
            get { return m_state == DrawerScrollState.Dragged; }
        }

        public bool IsEnabled
        {
            get { return m_state == DrawerScrollState.Disabled; }
        }

        public float ScrollVelocity
        {
            get { return m_scrollVelocity; }
            set { m_scrollVelocity = value; }
        }

        public float TopBound
        {
            get { return m_drawerTop; }
        }

        public float BottomBound
        { 
            get { return m_drawerBottom; } 
        }
        #endregion
    }
}