import React, { useEffect, useState } from 'react';
import { db } from '../firebaseConfig';
import { ref, onValue, push, set } from 'firebase/database';
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
        // Convert object to array and sort by time (newest first)
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
    <div className="features-grid" style={{marginTop: '2rem'}}>
      <div className="glass-card" style={{gridColumn: '1 / -1'}}>
        <h2>💬 Community Feed</h2>
        
        {currentUser ? (
          <div style={{display: 'flex', gap: '10px', marginTop: '1rem', marginBottom: '2rem'}}>
            <input 
              type="text" 
              value={newPost}
              onChange={(e) => setNewPost(e.target.value)}
              placeholder="Share your fitness milestone..." 
              style={{flex: 1, padding: '10px', borderRadius: '5px', border: '1px solid #ccc'}}
            />
            <button className="cta-button" onClick={handlePost}>Post</button>
          </div>
        ) : (
          <p style={{marginBottom: '2rem'}}><em>Log in to post your milestones!</em></p>
        )}

        <div style={{display: 'flex', flexDirection: 'column', gap: '15px'}}>
          {posts.map(post => (
            <div key={post.id} style={{background: 'rgba(255,255,255,0.1)', padding: '15px', borderRadius: '10px', textAlign: 'left'}}>
              <strong>{post.author}</strong> <span style={{fontSize: '0.8rem', color: '#ccc'}}>{new Date(post.timestamp).toLocaleString()}</span>
              <p style={{marginTop: '5px'}}>{post.text}</p>
            </div>
          ))}
          {posts.length === 0 && <p>No posts yet. Be the first!</p>}
        </div>
      </div>
    </div>
  );
}
