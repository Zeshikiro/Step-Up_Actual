import { onValue, push, ref, set } from 'firebase/database';
import { MessageCircle, Send } from 'lucide-react';
import { useEffect, useState } from 'react';
import { db } from '../firebaseConfig';
import { useAuth } from './AuthContext';

export default function SocialFeed() {
  const [posts, setPosts] = useState([]);
  const [newPost, setNewPost] = useState("");
  const { currentUser } = useAuth();

  useEffect(() => {
    const postsRef = ref(db, 'posts');
    onValue(postsRef, (snapshot) => {
      const data = snapshot.val();
      if (data) {
        const postList = Object.keys(data).map(key => ({
          id: key,
          ...data[key]
        })).sort((a, b) => b.timestamp - a.timestamp);

        setPosts(postList);
      }
    });
  }, []);

  const handlePost = () => {
    if (!newPost.trim() || !currentUser) return;

    const postsRef = ref(db, 'posts');
    const newPostRef = push(postsRef);
    set(newPostRef, {
      author: currentUser.email.split('@')[0],
      text: newPost,
      timestamp: Date.now()
    });
    setNewPost("");
  };

  return (
    <div style={{
        width: 'min(850px, calc(100vw - 40px))',
        margin: '2rem auto 0',
        paddingBottom: '2.5rem'
}}>
      <div style={{
        background: '#fff4d6',
        border: '5px solid #171717',
        borderRadius: '22px',
        padding: '2rem',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>

        {/* Orange title board */}
        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          maxWidth: '460px',
          margin: '0 auto 2rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '"Press Start 2P", cursive',
            fontSize: 'clamp(0.9rem, 2.5vw, 1.3rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff'
          }}>
            <MessageCircle size={24} color="#ffffff" style={{ verticalAlign: 'middle', marginRight: '10px' }} />
            COMMUNITY FEED
          </h2>
        </div>

        {/* Post box */}
        {currentUser ? (
          <div style={{
            display: 'flex',
            gap: '12px',
            marginBottom: '2rem',
            background: '#fff9e9',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1rem',
            boxShadow: '5px 5px 0 rgba(0,0,0,0.25)'
          }}>
            <input
              type="text"
              value={newPost}
              onChange={(e) => setNewPost(e.target.value)}
              placeholder="Share your fitness milestone..."
              style={{
                flex: 1,
                padding: '14px',
                fontSize: '1rem',
                borderRadius: '12px',
                border: '4px solid #171717',
                background: '#ffffff',
                color: '#1b2433'
              }}
            />

            <button
              onClick={handlePost}
              style={{
                background: '#3fd66b',
                color: '#082313',
                border: '4px solid #171717',
                borderRadius: '12px',
                padding: '0 18px',
                cursor: 'pointer',
                fontWeight: '900',
                boxShadow: '0 5px 0 #137333',
                display: 'flex',
                alignItems: 'center',
                gap: '8px'
              }}
            >
              <Send size={18} />
              Post
            </button>
          </div>
        ) : (
          <div style={{
            marginBottom: '2rem',
            background: '#fff9e9',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1rem',
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            boxShadow: '5px 5px 0 rgba(0,0,0,0.25)'
          }}>
            <div style={{
              width: '46px',
              height: '46px',
              background: '#f59b35',
              border: '3px solid #171717',
              borderRadius: '10px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: '900'
            }}>
              !
            </div>
            <p style={{
              margin: 0,
              color: '#1b2433',
              fontSize: '1.05rem',
              fontWeight: '700'
            }}>
              Log in to post your milestones!
            </p>
          </div>
        )}

        {/* Posts */}
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '16px'
        }}>
          {posts.map(post => (
            <div
              key={post.id}
              style={{
                background: '#fff9e9',
                border: '4px solid #171717',
                borderRadius: '16px',
                padding: '1rem',
                textAlign: 'left',
                boxShadow: '5px 5px 0 rgba(0,0,0,0.25)'
              }}
            >
              <div style={{
                display: 'flex',
                alignItems: 'center',
                gap: '12px',
                marginBottom: '10px'
              }}>
                <div style={{
                  width: '44px',
                  height: '44px',
                  background: '#ffd84d',
                  border: '3px solid #171717',
                  borderRadius: '10px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontWeight: '900',
                  fontSize: '1.2rem',
                  color: '#171717'
                }}>
                  {post.author ? post.author.charAt(0).toUpperCase() : 'U'}
                </div>

                <div style={{ flex: 1 }}>
                  <strong style={{
                    color: '#9b531e',
                    fontSize: '1.05rem'
                  }}>
                    {post.author}
                  </strong>

                  <div style={{
                    fontSize: '0.85rem',
                    color: '#4b5563',
                    fontWeight: '700'
                  }}>
                    {new Date(post.timestamp).toLocaleString()}
                  </div>
                </div>
              </div>

              <p style={{
                margin: 0,
                color: '#1b2433',
                lineHeight: '1.55',
                fontSize: '1.05rem'
              }}>
                {post.text}
              </p>
            </div>
          ))}

          {posts.length === 0 && (
            <div style={{
              background: '#fff9e9',
              border: '4px solid #171717',
              borderRadius: '16px',
              padding: '2rem',
              textAlign: 'center'
            }}>
              <p style={{
                margin: 0,
                color: '#1b2433',
                fontWeight: '800',
                fontSize: '1.1rem'
              }}>
                No posts yet. Be the first!
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}