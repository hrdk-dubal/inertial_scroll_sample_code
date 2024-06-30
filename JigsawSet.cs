using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JigsawFeud.TouchSystem;

namespace JigsawFeud.JigsawPuzzle
{
    public class JigsawSet : MonoBehaviour
    {
        [SerializeField]
        private short m_rows;

        [SerializeField]
        private short m_cols;

        private Vector3 m_dragBeginPiecePosition;
        private Dictionary<int, GameObject> m_jigsawPiecesGameObjects;

        // Start is called before the first frame update
        void Start()
        {
            Input.simulateMouseWithTouches = true;
            m_jigsawPiecesGameObjects = new Dictionary<int, GameObject>();
            TouchSystemEvents.PieceDragBeginEvent += HandlePieceDragBeginEvent;
            TouchSystemEvents.PieceDragMoveEvent += HandlePieceDragMoveEvent;
            TouchSystemEvents.PieceDragBeginFromDrawerEvent += HandlePieceDragBeginFromDrawerEvent;
            TouchSystemEvents.PieceDragEndEvent += HandlePieceDragEndEvent;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddJigsawPiece(int correspondingCellID, GameObject pieceGameObject)
        {
            JigsawPiece pieceObject = pieceGameObject.AddComponent<JigsawPiece>();
            pieceObject.Initialize(correspondingCellID);
            m_jigsawPiecesGameObjects[correspondingCellID] = pieceGameObject;
        }

        public void SetPuzzleImage(Texture puzzleImageTexture)
        {
            foreach (GameObject jigsawPieceGameObject in m_jigsawPiecesGameObjects.Values)
            {
                JigsawPiece jigsawPiece = jigsawPieceGameObject.GetComponent<JigsawPiece>();
                jigsawPiece.SetPuzzleImage(puzzleImageTexture);
            }
        }

        public List<JigsawPiece> GetJigsawPieceGameObjects(bool sequential = true)
        {
            List<JigsawPiece> selectedJigsawPieceGameObjects = new List<JigsawPiece>();
            
            
            if (sequential)
            {
                foreach (GameObject jigsawPieceGameObject in m_jigsawPiecesGameObjects.Values)
                {
                    selectedJigsawPieceGameObjects.Add(jigsawPieceGameObject.GetComponent<JigsawPiece>());
                }
            }
            else
            {
                List<GameObject> jigsawPieceGameObjects = new List<GameObject>();
                foreach (GameObject jigsawPieceGameObject in m_jigsawPiecesGameObjects.Values)
                {
                    jigsawPieceGameObjects.Add(jigsawPieceGameObject);
                }
                while (jigsawPieceGameObjects.Count > 0)
                {
                    int i = Random.Range(0, jigsawPieceGameObjects.Count);
                    selectedJigsawPieceGameObjects.Add(jigsawPieceGameObjects[i].GetComponent<JigsawPiece>());
                    jigsawPieceGameObjects.RemoveAt(i);
                }
            }
            return selectedJigsawPieceGameObjects;
        }

        private void HandlePieceDragBeginEvent(JigsawPiece piece, Vector2 touchBeginPosition)
        {
            if (piece.PieceState == JigsawPieceState.AtRest)
            {
                m_dragBeginPiecePosition = piece.transform.position;
                /*
                 * Note: This is piece's position before the dragging began. Whereas touchBeginPosition is the position
                 * of touch or mouse at the beginning of piece-dragging.
                 */
                piece.PieceState = JigsawPieceState.Dragged;
            }
        }

        private void HandlePieceDragBeginFromDrawerEvent(JigsawPiece piece)
        {
            Drawer drawer = piece.ParentContainer;
            drawer.RemovePiece(piece);
        }

        private void HandlePieceDragMoveEvent(JigsawPiece piece, Vector2 touchBeginPosition, Vector2 latestTouchPosition)
        {
            Vector3 newPiecePosition = CalculatePieceMovmentBasedOnTouchPosition(latestTouchPosition, touchBeginPosition);
            piece.transform.position = newPiecePosition;
        }

        public void HandlePieceDragEndEvent(JigsawPiece piece, Vector2 touchBeginPosition, Vector2 latestTouchPosition)
        {
            Vector3 newPiecePosition = CalculatePieceMovmentBasedOnTouchPosition(latestTouchPosition, touchBeginPosition);
            piece.transform.position = newPiecePosition;
            piece.PieceState = JigsawPieceState.AtRest;
        }

        private Vector3 CalculatePieceMovmentBasedOnTouchPosition(Vector2 currentTouchPosition, Vector2 touchBeginPosition)
        {
            Vector3 currentTouchPosition3D = Camera.main.ScreenToWorldPoint(currentTouchPosition);
            Vector3 dragBeginPosition3D = Camera.main.ScreenToWorldPoint(touchBeginPosition);
            Vector3 diffInTouchPosition3D = currentTouchPosition3D - dragBeginPosition3D;
            Vector3 newPiecePosition = m_dragBeginPiecePosition + diffInTouchPosition3D;
            return newPiecePosition;
        }

        #region getters and setters

        public short Rows
        {
            get { return m_rows; }
        }

        public short Columns
        {
            get { return m_cols; }
        }
        #endregion
    }
}