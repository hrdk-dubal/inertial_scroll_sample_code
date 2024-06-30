using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JigsawFeud.JigsawPuzzle
{
    public enum JigsawPieceState
    {
        AtRest,
        Dragged,
        FixedOnBoard
    }
    public class JigsawPiece : MonoBehaviour
    {
        private int m_id;
        private JigsawPiece m_leftPiece;
        private JigsawPiece m_rightPiece;
        private JigsawPiece m_topPiece;
        private JigsawPiece m_bottomPiece;
        private JigsawPieceState m_currentState;
        Renderer m_Renderer;
        private Drawer m_drawerRef;
        private int m_IndexInDrawer;

        public void Initialize(int id)
        {
            m_id = id;
            m_leftPiece = m_rightPiece = m_topPiece = m_bottomPiece = null;
            m_currentState = JigsawPieceState.AtRest;
            m_Renderer = this.gameObject.GetComponent<Renderer>();
            m_drawerRef = null;
            m_IndexInDrawer = -1;
        }

        public void SetPuzzleImage(Texture puzzleImageTexture)
        {
            m_Renderer.material.SetTexture("_MainTex", puzzleImageTexture);
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public Vector3 GetJigsawSetScale()
        {
            Vector3 jigsawSetScale = this.gameObject.transform.parent.localScale;
            return jigsawSetScale;
        }

        public float GetVerticalHalfExtent() //Half of vertical bound
        {
            float verticalExtent = m_Renderer.bounds.extents.y;
            //Reverse-scaling by piece's z-scale (NOTE: xz plane is the plane for the puzzle set. so z-scale is used here)
            verticalExtent /= this.gameObject.transform.localScale.z;
            //Reverse-scaling by Jigsaw-Set's scale
            verticalExtent /= this.gameObject.transform.parent.localScale.z;
            return verticalExtent;
        }

        #region getters and setters

        public int ID
        {
            get { return m_id; }
        }

        public JigsawPieceState PieceState
        {
            get { return m_currentState; }
            set
            {
                m_currentState = value;
                switch(m_currentState)
                {
                    case JigsawPieceState.Dragged:
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, Constants.PIECE_Y_COORDINATE_DRAGGING, gameObject.transform.localPosition.z);
                        gameObject.transform.localScale = new Vector3(Constants.PIECE_DRAGGING_SCALE_VALUE, Constants.PIECE_DRAGGING_SCALE_VALUE, 1);
                        break;
                    case JigsawPieceState.AtRest:
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, Constants.PIECE_Y_COORDINATE_AT_REST, gameObject.transform.localPosition.z);
                        gameObject.transform.localScale = new Vector3(1, 1, 1);
                        break;
                    case JigsawPieceState.FixedOnBoard:
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, Constants.PIECE_Y_COORDINATE_AT_REST, gameObject.transform.localPosition.z);
                        gameObject.transform.localScale = new Vector3(1, 1, 1);
                        break;
                }
            }
        }

        public void SetParentContainer(Drawer parentContainer, int index)
        {
            m_drawerRef = parentContainer;
            m_IndexInDrawer = index;
        }

        public void ClearParentContainer()
        {
            m_drawerRef = null;
            m_IndexInDrawer = -1;
        }

        public Drawer ParentContainer
        {
            get { return m_drawerRef; }
        }

        public int IndexInDrawer
        {
            get { return m_IndexInDrawer; }
            set { m_IndexInDrawer = value; }
        }

        public bool IsInDrawer
        {
            get { return m_drawerRef != null; }
        }

        #endregion
    }
}
