using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JigsawFeud.JigsawPuzzle;
using System.Reflection;
using JigsawFeud.TouchSystem;

namespace JigsawFeud
{
    public class Drawer : MonoBehaviour
    {
        [SerializeField]
        private short m_maxPieces;

        [SerializeField]
        private float m_margin;

        [SerializeField]
        private float m_verticalGap;

        [SerializeField]
        private GameObject m_jigsawSet;

        private SpriteRenderer m_rendererObj;

        private List<JigsawPiece> m_listOfPieces;

        //TODO: Need to reconsider gap logic
        private int m_showingGapAtIndex;
        private float m_gapSize;

        private DrawerScroll m_drawerScroll;
        

        //TODO: While adding or removing pieces, check if scrolling is required. If not then reset positions

        // Start is called before the first frame update
        void Start()
        {
            if (m_maxPieces <= 0)
            {
                Debug.LogError("Max Pieces must have positive value");
            }
            if (m_jigsawSet == null)
            {
                Debug.LogError("JigsawSet Unassigned for " + this.name);
            }

            m_rendererObj = this.gameObject.GetComponent<SpriteRenderer>();
            m_listOfPieces = new List<JigsawPiece>();
            m_showingGapAtIndex = -1;
            m_gapSize = 0;

            Bounds containerBounds = m_rendererObj.bounds;
            Vector3 containerExtents = containerBounds.extents;
            Vector3 containerCenter = containerBounds.center;
            float drawerTop = -containerExtents.y;
            float drawerBottom = containerExtents.y;// * this.transform.localScale.y;
            drawerTop -= this.transform.localPosition.y;
            drawerBottom -= this.transform.localPosition.y;

            drawerBottom += m_jigsawSet.transform.localPosition.y;
            drawerTop += m_jigsawSet.transform.localPosition.y;
            
            drawerTop = drawerTop / m_jigsawSet.transform.localScale.z; //If JigsawSet is scaled, then the positioning should be reverse-scaled
            drawerBottom = drawerBottom / m_jigsawSet.transform.localScale.z;
            drawerTop += m_margin;
            drawerBottom -= m_margin;

            m_drawerScroll = new DrawerScroll(this.name, m_listOfPieces, drawerTop, drawerBottom);
        }

        public void DisableScrolling()
        {
            if (m_drawerScroll.IsEnabled)
            {
                m_drawerScroll.DisableScrolling();
                ResetAllPiecesPositions();
            }
        }

        public void ToggleScrolling()
        {
            if (m_drawerScroll.IsEnabled)
            {
                DisableScrolling();
            }
            else
            {
                EnableScrolling();
            }
        }

        public void EnableScrolling()
        {
            if (!m_drawerScroll.IsEnabled) 
            {
                m_drawerScroll.EnableScrolling();
            }
        }

        // Update is called once per frame
        void Update()
        {
            m_drawerScroll.Update();
        }

        void LateUpdate()
        {
            m_drawerScroll.LateUpdate();
        }

        private void ResetAllPiecesPositions()
        {
            ArrangePieces(0, m_listOfPieces.Count);
        }

        public int DistributePieces(List<JigsawPiece> jigsawPieces, int startIndex)
        {
            int endIndex = startIndex + m_maxPieces;
            if (endIndex > jigsawPieces.Count)
            {
                endIndex = jigsawPieces.Count;
            }
            for (int i = startIndex; i < endIndex; i++)
            {
                JigsawPiece jigsawPiece = jigsawPieces[i];
                AddPiece(jigsawPiece);
            }
            return endIndex;
        }

        private void PositionPieceByIndex(int index)
        {
            if (index >= m_listOfPieces.Count)
            {
                return;
            }
            JigsawPiece jigsawPiece = m_listOfPieces[index];
            jigsawPiece.IndexInDrawer = index;
            if (index == 0)
            {
                Bounds containerBounds = m_rendererObj.bounds;
                Vector3 containerCenter = containerBounds.center;

                float localPositionZ = m_drawerScroll.TopBound;
                if (m_showingGapAtIndex == 0)
                {
                    localPositionZ += m_gapSize;
                }
                localPositionZ += jigsawPiece.GetVerticalHalfExtent();
                //Moving the piece to the center of the container
                jigsawPiece.transform.position = new Vector3(containerCenter.x,
                                                                          containerCenter.y,
                                                                          containerCenter.z);
                //Moving it locally so that the Z-order of the piece is maintained above the container (PIECE_Y_COORDINATE_AT_REST in this case) w.r.t. camera
                //Also arranging the piece vertically using Offset in z-axis (we see the xz plane through camera so z-axis is vertical)
                jigsawPiece.transform.localPosition = new Vector3(jigsawPiece.transform.localPosition.x,
                                                                            JigsawPuzzle.Constants.PIECE_Y_COORDINATE_AT_REST,
                                                                            localPositionZ);
            }
            else
            {
                JigsawPiece prevPiece = m_listOfPieces[index - 1];
                jigsawPiece.transform.position = prevPiece.transform.position;
                float localPositionZ = jigsawPiece.transform.localPosition.z + prevPiece.GetVerticalHalfExtent() + m_verticalGap + jigsawPiece.GetVerticalHalfExtent();
                if (m_showingGapAtIndex == index)
                {
                    localPositionZ += m_gapSize;
                }
                jigsawPiece.transform.localPosition = new Vector3(jigsawPiece.transform.localPosition.x,
                                                                    jigsawPiece.transform.localPosition.y,
                                                                    localPositionZ);
            }
        }

        public void RemovePiece(JigsawPiece pieceToRemove)
        {
            if (pieceToRemove.ParentContainer == this)
            {
                RemovePieceAt(pieceToRemove.IndexInDrawer);
            }
        }

        public JigsawPiece RemovePieceAt(int index)
        {
            if (index >= m_listOfPieces.Count)
            {
                return null;
            }

            JigsawPiece pieceToRemove = m_listOfPieces[index];
            Debug.Log("List count: " + m_listOfPieces.Count);

            m_listOfPieces.RemoveAt(index);
            pieceToRemove.ClearParentContainer();
            Debug.Log("List count after removal: " + m_listOfPieces.Count);
            int stIndex = index;
            int endIndex = m_listOfPieces.Count;
            ArrangePieces(stIndex, endIndex);
            return pieceToRemove;
        }

        public void AddPiece(JigsawPiece piece)
        {
            m_listOfPieces.Add(piece);
            piece.SetParentContainer(this, m_listOfPieces.Count - 1);
            ArrangePieces(m_listOfPieces.Count - 1, m_listOfPieces.Count);
        }

        public void AddPieceAt(JigsawPiece piece, int index)
        {
            if (index >= m_listOfPieces.Count)
            {
                AddPiece(piece);
            }
            else
            {
                m_listOfPieces.Insert(index, piece);
                piece.SetParentContainer(this, index);
                ArrangePieces(index, m_listOfPieces.Count);
            }
        }

        /*
         * stIndex included
         * endIndex not included
         */
        private void ArrangePieces(int stIndex, int endIndex)
        {
            //Debug.Log("Rearranging pieces from " + stIndex + " to " + endIndex);
            for (int i = stIndex; i < endIndex; i++)
            {
                PositionPieceByIndex(i);
            }
        }

        public void ShowGapForPieceAt(JigsawPiece piece, int index)
        {
            Debug.Log("Showing gap at index: " + index);
            if (index >= m_listOfPieces.Count)
            {
                index = m_listOfPieces.Count - 1;
            }
            m_showingGapAtIndex = index;
            m_gapSize = m_verticalGap + (2 * piece.GetVerticalHalfExtent()) + m_verticalGap;
            ArrangePieces(m_showingGapAtIndex, m_listOfPieces.Count);
        }

        public void RemoveGap()
        {
            Debug.Log("Removing gap at index: " + m_showingGapAtIndex);
            int gapWasAtIndex = m_showingGapAtIndex;
            m_showingGapAtIndex = -1;
            m_gapSize = 0;
            ArrangePieces(gapWasAtIndex, m_listOfPieces.Count);
        }

        public void StartScrollingWithVelocity(float velocity)
        {
            m_drawerScroll.StartScrollingWithVelocity(velocity);
        }

        #region temp
        public void OnStopScrollingButtonClicked(float duration)
        {
            StartCoroutine(StopScrollingWithEase(duration));
        }

        public IEnumerator StopScrollingWithEase(float duration)
        {
            float timeElapsed = 0;
            float startValue = m_drawerScroll.ScrollVelocity;
            while (timeElapsed < duration) 
            {
                Debug.Log("stopping");
                timeElapsed += Time.deltaTime;
                m_drawerScroll.ScrollVelocity = Mathf.Lerp(startValue, 0, timeElapsed / duration);
                yield return null; //wait for the next frame
            }

            m_drawerScroll.ScrollVelocity = 0;
        }

        #endregion

        public void StopScrollingImmediately()
        {
            StopCoroutine("StopScrollingWithEase");
            m_drawerScroll.StopScrollingImmediately();
        }

        #region getters and setters

        public short PiecesCapacity
        {
            get { return m_maxPieces; }
        }

        public bool IsGapDisplayed
        {
            get { return m_showingGapAtIndex >= 0; }
        }

        public Vector3 ScaleOfAssociatedJigsawSet
        {
            get { return m_jigsawSet.transform.localScale; }
        }

        public bool IsScrollingEnabled
        {
            get { return m_drawerScroll.IsEnabled; }
        }

        #endregion
    }
}
