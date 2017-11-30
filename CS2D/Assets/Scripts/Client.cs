using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {

	public int serverPort;
	public int clientPort;
	Channel channel;
	public PlayerController playerController;
	private SnapshotManager snapshotManager;
    CommunicationManager cm;
    public int playerId;
	public Object playerPrefab;
    public int snapshotBuffer = 1;
	public string ip;

	Dictionary<int,Player> players = new Dictionary<int,Player>();

	void Start() {
		snapshotManager = new SnapshotManager (snapshotBuffer);
        cm = new CommunicationManager();
        channel = new Channel(ip, clientPort, serverPort);
	}

	void OnDestroy() {
		channel.Disconnect();
	}

	void Update() {
		Packet s;
		if ((s = channel.GetPacket()) != null) {
			int messages = s.buffer.GetInt ();
			for (int i = 0; i < messages; i++) {
				Message m = readServerMessage (s.buffer);
				if (m != null) {
					ProcessMessage (m);
				} else {
					Debug.Log ("m is null");
				}
			}
			Debug.Log ("Recibi algo");
		}
        if (!snapshotManager.alreadyAdded) {
            snapshotManager.AddInterpolated();
        }

		if (Input.GetKeyDown(KeyCode.Space)) {
			//send player connect message
            ConnectPlayerMessage connectPlayerMessage = ConnectPlayerMessage.CreateConnectPlayerMessageToSend(playerId);
            cm.SendMessage(connectPlayerMessage);
		}
		processSnapshot ();
		Packet packet = Packet.Obtain();
		PlayerInputMessage pim = new PlayerInputMessage (playerId, playerController.playerInput);
		cm.SendMessage (pim);

        Packet p = cm.BuildPacket();
        channel.Send(p);
	}

	private Message readServerMessage(BitBuffer bf){
		MessageType messageType = bf.GetEnum<MessageType> ((int)MessageType.TOTAL);
		Message serverMessage = null;
		switch (messageType) {
		case MessageType.PLAYER_CONNECTED:
			serverMessage = PlayerConnectedMessage.CreatePlayerConnectedMessageToReceive();
			break;
		case MessageType.PLAYER_DISCONNECTED:
			serverMessage = PlayerDisconnectedMessage.CreatePlayerDisconnectedMessageToReceive ();
			break;
		case MessageType.SNAPSHOT:
			serverMessage = new SnapshotMessage();
			break;
		case MessageType.ACK_RELIABLE_MAX_WAIT_TIME:
			serverMessage = AckReliableMessage.CreateAckReliableMessageMessageToReceive ();
			break;
		case MessageType.ACK_RELIABLE_SEND_EVERY_PACKET:
			serverMessage = AckReliableSendEveryFrameMessage.CreateAckReliableSendEveryFrameMessageMessageToReceive ();
			break;
		default:
			Debug.LogError("Got a Server message that cannot be understood");
			return null;
		}
		serverMessage.Load(bf);
		return serverMessage;
	}

	private void ProcessMessage(Message message){
		switch (message.Type) {
		case MessageType.PLAYER_CONNECTED:
			Debug.Log ("Creating Player");
			newPlayer (((PlayerConnectedMessage)message).PlayerId);
			break;
		case MessageType.PLAYER_DISCONNECTED:
			removePlayer (((PlayerDisconnectedMessage)message).PlayerId);
			break;
		case MessageType.SNAPSHOT:
			snapshotManager.Add (((SnapshotMessage)message).GameSnapshot);
			break;
		case MessageType.ACK_RELIABLE_MAX_WAIT_TIME:
			
		case MessageType.ACK_RELIABLE_SEND_EVERY_PACKET:
			
			break;
		}
        cm.ReceiveMessage(message);
	}

	private void newPlayer(int otherId){
		Player p = (Instantiate (playerPrefab) as GameObject).GetComponent<Player> ();
		p.showVisualRepresentation = true;
		Debug.Log ("Created" + p);
		players.Add (otherId, p);
	}

	private void removePlayer(int otherId){
		players.Remove (otherId);
	}


	void processSnapshot () {
		Debug.Log ("Estoy buscando un snapshot");
		List<PlayerData> g = snapshotManager.get ();
		if (g != null) {
			Debug.Log ("Lo consegui");
			foreach (PlayerData p in g) {
				Debug.Log ("Hay un jugador");
				Player old;
				players.TryGetValue (p.PlayerId, out old);
				old.transform.position = p.Position;
			}
		} else {
			Debug.Log("NULL");
		}
	}
}
