import React from 'react';
import './BackgroundRunners.css';

export default function BackgroundRunners() {
  return (
    <div className="runners-container">
      <div className="runner man-runner">
        <img src="/man-runner.svg" alt="Running Man" />
      </div>
      <div className="runner woman-runner">
        <img src="/woman-runner.svg" alt="Running Woman" />
      </div>
    </div>
  );
}
