import React, { useState, useEffect } from 'react';
import AppealService from '../services/AppealService';

function AppealDetails({ appealId, onBack }) {
  const [appeal, setAppeal] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchDetails = async () => {
      setLoading(true);
      const result = await AppealService.getAppealDetails(appealId);
      if (result.success) {
        setAppeal(result.appeal);
      } else {
        setError(result.errors?.[0] || 'Failed to load appeal details.');
      }
      setLoading(false);
    };

    if (appealId) {
      fetchDetails();
    }
  }, [appealId]);

  if (loading) {
    return <div style={{ padding: '20px', textAlign: 'center' }}>Loading full details...</div>;
  }

  if (error || !appeal) {
    return (
        <div style={{ padding: '20px', textAlign: 'center', color: 'red' }}>
          <p>{error || 'Appeal not found.'}</p>
          <button onClick={onBack} style={backButtonStyle}>Back to Dashboard</button>
        </div>
    );
  }

  return (
      <div style={{ maxWidth: '800px', margin: '0 auto', padding: '20px', backgroundColor: 'white', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' }}>
        <button onClick={onBack} style={backButtonStyle}>← Back to Map</button>

        <div style={{ borderBottom: '1px solid #eee', paddingBottom: '15px', marginBottom: '20px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <h1 style={{ margin: '0 0 10px 0', color: '#333' }}>{appeal.title}</h1>
            <span style={{
              padding: '6px 12px',
              borderRadius: '20px',
              fontWeight: 'bold',
              fontSize: '14px',
              backgroundColor: appeal.status === 'PENDING' ? '#fff3cd' : appeal.status === 'RESOLVED' ? '#d4edda' : '#cce5ff',
              color: appeal.status === 'PENDING' ? '#856404' : appeal.status === 'RESOLVED' ? '#155724' : '#004085',
            }}>
            {appeal.status}
          </span>
          </div>
          <p style={{ color: '#666', margin: 0 }}>Reported by <strong>{appeal.userFullName}</strong> on {appeal.date}</p>
        </div>

        <div style={{ marginBottom: '25px' }}>
          <h3 style={{ marginTop: 0, color: '#444' }}>Description</h3>
          <p style={{ lineHeight: '1.6', color: '#333', backgroundColor: '#f9f9f9', padding: '15px', borderRadius: '4px' }}>
            {appeal.description}
          </p>
        </div>

        <div style={{ marginBottom: '25px' }}>
          <h3 style={{ marginTop: 0, color: '#444' }}>Location Details</h3>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '10px', backgroundColor: '#f9f9f9', padding: '15px', borderRadius: '4px' }}>
            <div><strong>City:</strong> {appeal.city || 'N/A'}</div>
            <div><strong>Street:</strong> {appeal.street || 'N/A'}</div>
            <div><strong>Building:</strong> {appeal.buildingNumber || 'N/A'}</div>
            <div><strong>Coordinates:</strong> {appeal.latitude?.toFixed(6) || 'N/A'}, {appeal.longitude?.toFixed(6) || 'N/A'}</div>
          </div>
        </div>

        <div>
          <h3 style={{ marginTop: 0, color: '#444' }}>Photos</h3>
          {appeal.photos && appeal.photos.length > 0 ? (
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '15px' }}>
                {appeal.photos.map((imgSrc, index) => (
                    <div key={index} style={{ border: '1px solid #ddd', borderRadius: '4px', overflow: 'hidden' }}>
                      <img
                          src={imgSrc}
                          alt={`Appeal evidence ${index + 1}`}
                          style={{ width: '100%', height: '200px', objectFit: 'cover', display: 'block' }}
                      />
                    </div>
                ))}
              </div>
          ) : (
              <p style={{ color: '#777', fontStyle: 'italic' }}>No photos were attached to this appeal.</p>
          )}
        </div>
      </div>
  );
}

const backButtonStyle = {
  backgroundColor: '#f1f3f4',
  border: 'none',
  padding: '8px 16px',
  borderRadius: '4px',
  cursor: 'pointer',
  fontWeight: 'bold',
  color: '#333',
  marginBottom: '20px',
  display: 'inline-block'
};

export default AppealDetails;